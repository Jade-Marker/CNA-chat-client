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
        private List<HangmanGame> _hangmanGames;

        public Server(string ipAddress, int port)
        {
            _tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
        }

        public void Start()
        {
            _clients = new ConcurrentSet<Client>();
            _tcpListener.Start();

            _hangmanGames = new List<HangmanGame>();

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

                        client.SetClientKey((receivedMessage as ConnectionPacket).publicKey);

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
                                    List<string> clientList;
                                    List<string> returnMessage = GetReturnMessage(message, client, out sendToAllClients, out clientList);

                                    if (sendToAllClients)
                                        SendToAllClients(new ChatMessagePacket(returnMessage));
                                    else
                                    {
                                        SendToSpecificClients(new ChatMessagePacket(returnMessage), clientList);
                                    }
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

        private List<string> GetReturnMessage(string code, Client client, out bool sendToAllClients, out List<string> clientsToSendTo)
        {
            List<string> response = new List<string>();

            string command = ExtractCommand(code);
            string arguments = ExtractArguments(code, command);

            bool prependServer = true;
            sendToAllClients = false;

            clientsToSendTo = new List<string>() { client.Name };

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
                    lock (_hangmanGames)
                    {
                        HandleHangmanGame(client, ref clientsToSendTo, ref response, arguments);
                    }

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

        private void SendToSpecificClients(Packet packet, List<string> clientsToSendTo)
        {
            foreach (Client currClient in _clients)
            {
                if (clientsToSendTo.Contains(currClient.Name))
                    currClient.SendEncrypted(packet);
            }
        }

        private void HandleHangmanGame(Client client, ref List<string> clientsToSendTo, ref List<string> response, string arguments)
        {
            HangmanGame hangmanGame;
            bool isInGame = FindGameClientIsIn(out hangmanGame, client.Name);

            if (isInGame && hangmanGame.GameRunning)
            {
                clientsToSendTo.Remove(client.Name);                //Remove client so that when adding Players, client isn't repeated
                clientsToSendTo.AddRange(hangmanGame.Players);
                response = hangmanGame.Guess(arguments, client.Name);


                if (!hangmanGame.GameRunning)                       //If game is not running here, then the game just ended, so it can be removed from hangmanGames
                    _hangmanGames.Remove(hangmanGame);
            }
            else
            {
                switch (arguments)
                {
                    case "":
                        HangmanHandleEmpty(client, clientsToSendTo, response, hangmanGame);
                        break;

                    case "join":
                        HangmanHandleJoin(client, clientsToSendTo, response, isInGame);
                        break;

                    case "start":
                        HangmanHandleStart(client, clientsToSendTo, response);
                        break;

                    default:
                        response.Add("Sorry, I don't understand the argument " + arguments);
                        break;
                }
            }
        }

        private void HangmanHandleEmpty(Client client, List<string> clientsToSendTo, List<string> response, HangmanGame hangmanGame)
        {
            int currentGame = _hangmanGames.Count - 1;

            if (IsGameAvailable())
            {
                if (hangmanGame == _hangmanGames[currentGame])
                    response.Add("You are already in that game");
                else
                    JoinGame(_hangmanGames[currentGame], client.Name, response, clientsToSendTo);
            }
            else
            {
                CreateHangmanGame(response, client.Name);
                clientsToSendTo.AddRange(GetClientsNotInGames());
            }
        }

        private void HangmanHandleJoin(Client client, List<string> clientsToSendTo, List<string> response, bool isInGame)
        {
            int currentGame = _hangmanGames.Count - 1;

            if (isInGame)
                response.Add("You are already in a game");
            else
            {
                if (IsGameAvailable())
                    JoinGame(_hangmanGames[currentGame], client.Name, response, clientsToSendTo);
                else
                    response.Add("No game available to join. Try /game to start a new game");
            }
        }

        private void HangmanHandleStart(Client client, List<string> clientsToSendTo, List<string> response)
        {
            int currentGame = _hangmanGames.Count - 1;

            if (IsGameAvailable())
            {
                if (_hangmanGames[currentGame].Players[0] == client.Name)
                {
                    _hangmanGames[currentGame].StartGame();
                    response.AddRange(_hangmanGames[currentGame].GetBoard());
                    clientsToSendTo.AddRange(_hangmanGames[currentGame].Players);
                }
                else
                    response.Add("Only the host can start the game");
            }
            else
                response.Add("No game available to start");
        }

        private void CreateHangmanGame(List<string> response, string client)
        {
            HangmanGame hangman = new HangmanGame();
            _hangmanGames.Add(hangman);
            hangman.Starting();
            hangman.AddPlayer(client);
            response.Add("New game of hangman started by " + client);
            response.Add("Type /game join to join");
            response.Add("The host can type /game start to start");
        }

        private void JoinGame(HangmanGame hangmanGame, string client, List<string> response, List<string> clientsToSendTo)
        {
            hangmanGame.AddPlayer(client);
            response.Add(client + " has joined");
            clientsToSendTo.AddRange(hangmanGame.Players);
        }

        private bool IsGameAvailable()
        {
            return (_hangmanGames.Count > 0 && _hangmanGames[_hangmanGames.Count - 1].GameStarting);
        }

        private List<string> GetClientsNotInGames()
        {
            List<string> clientsNotInGames = new List<string>();

            List<string> clientsInGames = new List<string>();
            foreach (HangmanGame hangmanGame in _hangmanGames)
            {
                clientsInGames.AddRange(hangmanGame.Players);
            }

            foreach (Client currClient in _clients)
            {
                if (!clientsInGames.Contains(currClient.Name))
                {
                    clientsNotInGames.Add(currClient.Name);
                }
            }

            return clientsNotInGames;
        }

        private bool FindGameClientIsIn(out HangmanGame hangmanGame, string clientName)
        {
            foreach (HangmanGame hangman in _hangmanGames)
            {
                if (hangman.Players.Contains(clientName))
                {
                    hangmanGame = hangman;
                    return true;
                }
            }

            hangmanGame = null;
            return false;
        }
    }
}
