using Packets;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    class Server
    {
        private TcpListener _tcpListener;
        private ConcurrentSet<Client> _clients;

        public Server(string ipAddress, int port)
        {
            _tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
        }

        public void Start()
        {
            _clients = new ConcurrentSet<Client>();
            _tcpListener.Start();

            while (true)
            {
                Socket socket = _tcpListener.AcceptSocket();

                Client client = new Client(socket);
                _clients.TryAdd(client);

                Thread thread = new Thread(() => { ClientMethod(client); });
                thread.Start();
            }
        }

        public void Stop()
        {
            _tcpListener.Stop();
        }

        private void ClientMethod(Client client)
        {
            Packet receivedMessage;
            client.Send(new ChatMessagePacket("Server: " + "You have connected to the server"));

            while ((receivedMessage = client.Read()) != null)
            {
                switch (receivedMessage.packetType)
                {
                    case PacketType.CHAT_MESSAGE:
                        string message = ((ChatMessagePacket)receivedMessage).message;
                        if (message.StartsWith("/"))
                        {
                            string returnMessage = GetReturnMessage(message, client);

                            client.Send(new ChatMessagePacket(returnMessage));
                        }
                        else
                        {
                            foreach(Client currClient in _clients)
                            {
                                currClient.Send(new ChatMessagePacket(client.Name + ": " + message));
                            }
                        }
                        break;

                    case PacketType.CONNECTION_START:
                        string newName = ValidateName(((ConnectionPacket)receivedMessage).name);                        

                        client.ChangeName(newName);
                        foreach (Client currClient in _clients)
                        {
                            if (currClient != client)
                            { 
                                currClient.Send(new ChatMessagePacket(newName + " has connected"));
                                currClient.Send(new ClientConnectPacket(newName));
                            }
                        }

                        List<string> clientNames = new List<string>();
                        foreach (Client currClient in _clients)
                        {
                            clientNames.Add(currClient.Name);
                        }
                        client.Send(new ClientListPacket(clientNames));
                        break;

                    case PacketType.PRIVATE_MESSAGE:
                        string name = ((PrivateMessagePacket)receivedMessage).name;
                        string privateMessage = ((PrivateMessagePacket)receivedMessage).message;

                        if (name == client.Name)
                        {
                            client.Send(new ChatMessagePacket("You cannot pm yourself"));
                        }
                        else
                        {
                            bool clientFound = false;
                            foreach (Client currClient in _clients)
                            {
                                if (currClient.Name == name)
                                {
                                    currClient.Send(new ChatMessagePacket("[" + client.Name + "]: " + privateMessage));
                                    clientFound = true;
                                    break;
                                }
                            }

                            if (clientFound)
                                client.Send(new ChatMessagePacket("[" + client.Name + "]: " + privateMessage));
                            else
                                client.Send(new ChatMessagePacket(name + " was not found"));
                        }

                        break;
                }
            }

            client.Close();
            _clients.TryRemove(client);

            foreach (Client currClient in _clients)
            {
                currClient.Send(new ChatMessagePacket(client.Name + " disconnected"));
                currClient.Send(new ClientDisconnectPacket(client.Name));
            }
        }

        private string GetReturnMessage(string code, Client client)
        {
            string response = "";

            string command = ExtractCommand(code);
            string arguments = ExtractArguments(code, command);

            bool prependServer = true;

            switch (command)
            {
                case "/hi":
                    if (arguments == "")
                        response = "Hello";
                    else
                        response = "/hi takes no arguments";
                    break;
                case "/test":
                    response = "New test";
                    break;

                default:
                    response = "Sorry, I don't understand the command " + code;
                    break;
            }

            if (prependServer)
                response = "Server: " + response;

            return response;
        }

        private string ExtractCommand(string code)
        {
            string command = "";
            if (code.StartsWith("/"))
            {
                command = code.Split(' ')[0];
            }

            return command;
        }
        private string ExtractArguments(string code, string command)
        {
            if (command.Length + 1 > code.Length)
                return "";
            else
                return code.Substring(command.Length + 1);
        }

        private string ValidateName(string name)
        {
            string newName = name;

            List<string> names = new List<string>();
            foreach (Client client in _clients)
                names.Add(client.Name);

            int loopCounter = 1;
            while (names.Contains(newName))
            {
                newName = name + "(" + loopCounter + ")";

                loopCounter++;
            }

            return newName;
        }
    }
}
