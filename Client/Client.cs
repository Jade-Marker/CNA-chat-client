using Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ClientNamespace
{
    public class Client
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private BinaryWriter writer;
        private BinaryReader reader;
        private BinaryFormatter formatter;
        private ClientForm clientForm;
        public Client()
        {
            clientForm = new ClientForm(this);
            clientForm.ShowDialog();
        }

        public bool Connect(string ipAddress, int port)
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(IPAddress.Parse(ipAddress), port);
                stream = tcpClient.GetStream();
                writer = new BinaryWriter(stream);
                reader = new BinaryReader(stream);
                formatter = new BinaryFormatter();
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
            Thread thread = new Thread(() => 
            {
                ProcessServerResponse();
            });
            thread.Start();       
        }

        private void ProcessServerResponse()
        {
            Packet serverResponse;
            try
            {
                while ((serverResponse = Read()) != null)
                {
                    switch (serverResponse.packetType)
                    {
                        case PacketType.CHAT_MESSAGE:
                            clientForm.UpdateChatWindow(((ChatMessagePacket)serverResponse).message);
                            break;
                    }
                }
            }
            catch (System.IO.IOException)
            {
                
            }

        }

        public void Close()
        {
            if (tcpClient.Connected)
            {
                writer.Write(-1);
                writer.Flush();

                tcpClient.Close();
            }
        }

        private Packet Read()
        {
            return Packet.ReadPacket(reader, formatter);
        }

        public void SendMessage(Packet message)
        {
            Packet.SendPacket(message, formatter, writer);
        }
    }
}
