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
                    string returnMessage = GetReturnMessage(receivedMessage);

                    client.Send("Server: " + returnMessage);
                }
                else
                {
                    foreach (Client currClient in _clients)
                    {
                        currClient.Send(receivedMessage);
                    }
                }
            }

            client.Close();

            _clients.TryTake(out client);
        }

        private string GetReturnMessage(string code)
        {
            string response;

            switch (code)
            {
                case "/hi":
                    response = "Hello";
                    break;
                case "/test":
                    response = "New test";
                    break;

                default:
                    response = "Sorry, I don't understand the command " + code;
                    break;
            }


            return response;
        }
    }
}
