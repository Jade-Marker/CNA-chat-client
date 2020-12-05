﻿using Packets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
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
        private RSACryptoServiceProvider RSAProvider;
        private RSAParameters PrivateKey;
        private RSAParameters ServerKey;
        public RSAParameters PublicKey { get; private set; }

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
                RSAProvider = new RSACryptoServiceProvider(Encryption.KeySize);
                PublicKey = RSAProvider.ExportParameters(false);
                PrivateKey = RSAProvider.ExportParameters(true);
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
                        case PacketType.SERVER_KEY:
                            ServerKeyPacket serverKeyPacket = serverResponse as ServerKeyPacket;
                            ServerKey = serverKeyPacket.PublicKey;
                            break;

                        case PacketType.ENCRYPTED:
                            Packet decrypted = EncryptedPacket.DecryptPacket(serverResponse as EncryptedPacket, formatter, PrivateKey, RSAProvider);

                            switch (decrypted.packetType)
                            {
                                case PacketType.CHAT_MESSAGE:
                                    ChatMessagePacket chatMessagePacket = decrypted as ChatMessagePacket;
                                    foreach (string message in chatMessagePacket.messages)
                                        clientForm.UpdateChatWindow(message);

                                    break;

                                case PacketType.CLIENT_CONNECT:
                                    clientForm.UpdateClientList((decrypted as ClientConnectPacket).name);
                                    break;

                                case PacketType.CLIENT_DISCONNECT:
                                    clientForm.RemoveClient((decrypted as ClientDisconnectPacket).name);
                                    break;

                                case PacketType.CLIENT_LIST:
                                    foreach (string name in (decrypted as ClientListPacket).clientNames)
                                    {
                                        clientForm.UpdateClientList(name);
                                    }
                                    break;
                            }

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
            if (tcpClient != null && tcpClient.Connected)
            {
                writer.Write(-1);
                writer.Flush();

                tcpClient.Close();

                clientForm.ClearClientList();
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

        public void SendEncrypted(Packet message)
        {
            Packet.SendPacket(EncryptedPacket.EncryptPacket(message, formatter, ServerKey, RSAProvider), formatter, writer);
        }
    }
}
