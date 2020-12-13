using Packets;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Threading;

namespace ClientNamespace
{
    public class Client
    {
        public RSAParameters publicKey { get; private set; }

        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private BinaryWriter _writer;
        private BinaryReader _reader;
        private BinaryFormatter _formatter;

        private ClientForm _clientForm;

        private RSACryptoServiceProvider _rsaProvider;
        private RSAParameters _privateKey;
        private RSAParameters _serverKey;

        public Client()
        {
            _clientForm = new ClientForm(this);
            _clientForm.ShowDialog();
        }

        public bool Connect(string ipAddress, int port)
        {
            try
            {
                _tcpClient = new TcpClient();
                _tcpClient.Connect(IPAddress.Parse(ipAddress), port);
                _stream = _tcpClient.GetStream();
                _writer = new BinaryWriter(_stream);
                _reader = new BinaryReader(_stream);
                _formatter = new BinaryFormatter();
                _rsaProvider = new RSACryptoServiceProvider(Encryption.cKeySize);
                publicKey = _rsaProvider.ExportParameters(false);
                _privateKey = _rsaProvider.ExportParameters(true);
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
                            _serverKey = serverKeyPacket.publicKey;
                            break;

                        case PacketType.ENCRYPTED:
                            Packet decrypted = EncryptedPacket.DecryptPacket(serverResponse as EncryptedPacket, _formatter, _privateKey, _rsaProvider);

                            switch (decrypted.packetType)
                            {
                                case PacketType.CHAT_MESSAGE:
                                    ChatMessagePacket chatMessagePacket = decrypted as ChatMessagePacket;
                                    foreach (string message in chatMessagePacket.messages)
                                        _clientForm.UpdateChatWindow(message, chatMessagePacket.profilePictureIndex);

                                    break;

                                case PacketType.CLIENT_CONNECT:
                                    _clientForm.UpdateClientList((decrypted as ClientConnectPacket).name);
                                    break;

                                case PacketType.CLIENT_DISCONNECT:
                                    _clientForm.RemoveClient((decrypted as ClientDisconnectPacket).name);
                                    break;

                                case PacketType.CLIENT_LIST:
                                    foreach (string name in (decrypted as ClientListPacket).clientNames)
                                    {
                                        _clientForm.UpdateClientList(name);
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
            if (_tcpClient != null && _tcpClient.Connected)
            {
                _writer.Write(-1);
                _writer.Flush();

                _tcpClient.Close();

                _clientForm.ClearClientList();
            }
        }

        private Packet Read()
        {
            return Packet.ReadPacket(_reader, _formatter);
        }

        public void SendMessage(Packet message)
        {
            Packet.SendPacket(message, _formatter, _writer);
        }

        public void SendEncrypted(Packet message)
        {
            Packet.SendPacket(EncryptedPacket.EncryptPacket(message, _formatter, _serverKey, _rsaProvider), _formatter, _writer);
        }
    }
}
