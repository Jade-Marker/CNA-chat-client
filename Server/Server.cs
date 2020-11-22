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
                        string newName = ((ConnectionPacket)receivedMessage).name;

                        client.ChangeName(newName);
                        foreach (Client currClient in _clients)
                        {
                            if (currClient != client)
                                currClient.Send(new ChatMessagePacket(newName + " has connected"));
                        }
                        break;
                }
            }

            client.Close();
            _clients.TryRemove(client);

            foreach (Client currClient in _clients)
            {
                currClient.Send(new ChatMessagePacket(client.Name + " disconnected"));
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

                case "/pm":
                    string name = arguments.Split(' ')[0];

                    if (name.Length + 1 >= arguments.Length)
                        response = "No message attached";
                    else
                    {

                        string message = arguments.Substring(name.Length + 1);

                        bool clientFound = false;

                        foreach (Client currClient in _clients)
                        {
                            if (currClient.Name == name)
                            {
                                currClient.Send(new ChatMessagePacket(client.Name + " says: " + message));
                                response = client.Name + ": " + message;
                                clientFound = true;
                                prependServer = false;
                            }

                            if (clientFound)
                                break;
                        }

                        if (!clientFound)
                            response = name + " was not found";
                    }

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
    }
}
