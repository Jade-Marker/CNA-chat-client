using Packets;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace Server
{
    public class Client
    {
        public RSAParameters publicKey { get; private set; }
        public string name { get { return _name; } private set { _name = value; } }

        private Socket _socket;
        private NetworkStream _stream;
        private BinaryReader _reader;
        private BinaryWriter _writer;
        private BinaryFormatter _formatter;

        private object _readLock;
        private object _writeLock;
        private string _name;

        private RSACryptoServiceProvider _rsaProvider;
        private RSAParameters _privateKey;
        private RSAParameters _clientKey;

        public Client(Socket socket)
        {
            _readLock = new object();
            _writeLock = new object();

            _socket = socket;

            _stream = new NetworkStream(_socket);
            _formatter = new BinaryFormatter();

            _reader = new BinaryReader(_stream);
            _writer = new BinaryWriter(_stream);

            _name = "";

            _rsaProvider = new RSACryptoServiceProvider(Encryption.cKeySize);
            publicKey = _rsaProvider.ExportParameters(false);
            _privateKey = _rsaProvider.ExportParameters(true);
        }

        public void Close()
        {
            _stream.Close();
            _reader.Close();
            _writer.Close();
            _socket.Close();
        }

        public Packet Read()
        {
            lock(_readLock)
            {
                return Packet.ReadPacket(_reader, _formatter);
            }
        }

        public void Send(Packet message)
        {
            lock (_writeLock)
            {
                Packet.SendPacket(message, _formatter, _writer);
            }
        }

        public void SendEncrypted(Packet message)
        {
            Send(Encrypt(message));
        }

        public EncryptedPacket Encrypt(Packet message)
        {
            return EncryptedPacket.EncryptPacket(message, _formatter, _clientKey, _rsaProvider);
        }

        public Packet Decrypt(EncryptedPacket message)
        {
            return EncryptedPacket.DecryptPacket(message, _formatter, _privateKey, _rsaProvider);
        }

        public void ChangeName(string name)
        {
            this.name = name;
        }

        public void SetClientKey(RSAParameters clientKey)
        {
            _clientKey = clientKey;
        }
    }
}
