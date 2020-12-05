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

        private HangmanGame hangman;

        public Server(string ipAddress, int port)
        {
            _tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
        }

        public void Start()
        {
            _clients = new ConcurrentSet<Client>();
            _tcpListener.Start();

            hangman = new HangmanGame();

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

            while ((receivedMessage = client.Read()) != null)
            {
                switch (receivedMessage.packetType)
                {
                    case PacketType.CONNECTION_START:
                        string newName = ValidateName(((ConnectionPacket)receivedMessage).name);

                        client.SetClientKey((receivedMessage as ConnectionPacket).PublicKey);

                        client.ChangeName(newName);
                        foreach (Client currClient in _clients)
                        {
                            if (currClient != client)
                            {
                                currClient.SendEncrypted(new ChatMessagePacket(newName + " has connected"));
                                currClient.SendEncrypted(new ClientConnectPacket(newName));
                            }
                        }

                        List<string> clientNames = new List<string>();
                        foreach (Client currClient in _clients)
                        {
                            clientNames.Add(currClient.Name);
                        }
                        client.Send(new ServerKeyPacket(client.PublicKey));
                        client.SendEncrypted(new ClientListPacket(clientNames));
                        client.SendEncrypted(new ChatMessagePacket("Server: " + "You have connected to the server"));

                        break;                   

                    case PacketType.ENCRYPTED:
                        Packet decrypted = client.Decrypt(receivedMessage as EncryptedPacket);

                        switch (decrypted.packetType)
                        {
                            case PacketType.CHAT_MESSAGE:
                                string message = ((ChatMessagePacket)decrypted).message;
                                if (message.StartsWith("/"))
                                {
                                    bool sendToAllClients;
                                    List<string> returnMessage = GetReturnMessage(message, client, out sendToAllClients);

                                    if (sendToAllClients)
                                        SendToAllClients(new ChatMessagePacket(returnMessage));
                                    else
                                        client.SendEncrypted(new ChatMessagePacket(returnMessage));
                                }
                                else
                                {
                                    SendToAllClients(new ChatMessagePacket(client.Name + ": " + message));
                                }
                                break;

                            case PacketType.PRIVATE_MESSAGE:
                                string name = ((PrivateMessagePacket)decrypted).name;
                                string privateMessage = ((PrivateMessagePacket)decrypted).message;

                                if (name == client.Name)
                                {
                                    client.SendEncrypted(new ChatMessagePacket("You cannot pm yourself"));
                                }
                                else
                                {
                                    bool clientFound = false;
                                    foreach (Client currClient in _clients)
                                    {
                                        if (currClient.Name == name)
                                        {
                                            currClient.SendEncrypted(new ChatMessagePacket("[" + client.Name + "]: " + privateMessage));
                                            clientFound = true;
                                            break;
                                        }
                                    }

                                    if (clientFound)
                                        client.SendEncrypted(new ChatMessagePacket("[" + client.Name + "]: " + privateMessage));
                                    else
                                        client.SendEncrypted(new ChatMessagePacket(name + " was not found"));
                                }
                                break;
                        }
                        break;
                }
            }

            client.Close();
            _clients.TryRemove(client);

            SendToAllClients(new ChatMessagePacket(client.Name + " disconnected"));
            SendToAllClients(new ClientDisconnectPacket(client.Name));

        }

        private List<string> GetReturnMessage(string code, Client client, out bool sendToAllClients)
        {
            List<string> response = new List<string>();

            string command = ExtractCommand(code);
            string arguments = ExtractArguments(code, command);

            bool prependServer = true;
            sendToAllClients = false;

            switch (command)
            {
                case "/hi":
                    if (arguments == "")
                        response.Add("Hello");
                    else
                        response.Add("/hi takes no arguments");
                    break;
                case "/test":
                    response.Add("New test");
                    break;

                case "/game":
                    if (hangman.GameRunning)
                    {
                        response = hangman.Guess(arguments, client.Name);
                    }
                    else
                    {
                        response.Add("New game of hangman started by " + client.Name);

                        hangman.StartGame();

                        response.AddRange(hangman.GetBoard());
                    }

                    sendToAllClients = true;
                    prependServer = false;
                    break;

                default:
                    response.Add("Sorry, I don't understand the command " + code);
                    break;
            }

            if (prependServer)
            {
                for(int i = 0; i < response.Count; i++)
                    response[i] = "Server: " + response[i];
            }

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

        private void SendToAllClients(Packet packet)
        {
            foreach (Client currClient in _clients)
            {
                currClient.SendEncrypted(packet);
            }
        }
    }
}
