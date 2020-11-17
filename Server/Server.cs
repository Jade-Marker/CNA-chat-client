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
        private ConcurrentBag<Client> _clients;

        public Server(string ipAddress, int port)
        {
            _tcpListener = new TcpListener(IPAddress.Parse(ipAddress), port);
        }

        public void Start()
        {
            _clients = new ConcurrentBag<Client>();
            _tcpListener.Start();

            while (true)
            {
                Socket socket = _tcpListener.AcceptSocket();

                Client client = new Client(socket);
                _clients.Add(client);

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
            string receivedMessage;
            client.Send("Server: " + "You have connected to the server");

            while ((receivedMessage = client.Read()) != null)
            {
                if (receivedMessage.StartsWith("/"))
                {
                    string returnMessage = GetReturnMessage(receivedMessage, client);

                    client.Send("Server: " + returnMessage);
                }
                else
                {
                    foreach (Client currClient in _clients)
                    {
                        currClient.Send(client.Name + ": " + receivedMessage);
                    }
                }
            }

            client.Close();

            _clients.TryTake(out client);
        }

        private string GetReturnMessage(string code, Client client)
        {
            string response;

            string command = ExtractCommand(code);
            string arguments = ExtractArguments(code, command);

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

                case "/name":
                    client.ChangeName(arguments);
                    response = "Name changed to " + arguments;
                    break;

                default:
                    response = "Sorry, I don't understand the command " + code;
                    break;
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
                return code.Substring(command.Length + 1, code.Length - command.Length -1);
        }
    }
}
