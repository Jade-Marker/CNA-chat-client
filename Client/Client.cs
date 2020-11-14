using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientNamespace
{
    public class Client
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private StreamWriter writer;
        private StreamReader reader;
        private ClientForm clientForm;
        public Client()
        {
            tcpClient = new TcpClient();
        }

        public bool Connect(string ipAddress, int port)
        {
            try
            {
                tcpClient.Connect(IPAddress.Parse(ipAddress), port);
                stream = tcpClient.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
                return false;
            }
        }

        public void Run()
        {
            clientForm = new ClientForm(this);

            Thread thread = new Thread(() => 
            {
                ProcessServerResponse();
            });
            thread.Start();
            clientForm.ShowDialog();

            tcpClient.Close();
        }

        private void ProcessServerResponse()
        {
            string serverResponse;
            try
            {
                while ((serverResponse = reader.ReadLine()) != null)
                {
                    clientForm.UpdateChatWindow(serverResponse);
                }
            }
            catch (System.IO.IOException)
            {
                
            }

        }

        public void SendMessage(string message)
        {
            writer.WriteLine(message);
            writer.Flush();
        }
    }
}
