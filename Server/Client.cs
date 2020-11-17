using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class Client
    {
        private Socket _socket;
        private NetworkStream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;
        private object _readLock;
        private object _writeLock;

        private string _name;
        public string Name { get { return _name; } private set { _name = value; } }

        public Client(Socket socket)
        {
            _readLock = new object();
            _writeLock = new object();

            _socket = socket;

            _stream = new NetworkStream(_socket);
            _reader = new StreamReader(_stream, Encoding.UTF8);
            _writer = new StreamWriter(_stream, Encoding.UTF8);

            _name = "User";
        }

        public void Close()
        {
            _stream.Close();
            _reader.Close();
            _writer.Close();
            _socket.Close();
        }

        public string Read()
        {
            lock(_readLock)
            {
                return _reader.ReadLine();
            }
        }

        public void Send(string message)
        {
            lock (_writeLock)
            {
                _writer.WriteLine(message);
                _writer.Flush();
            }
        }

        public void ChangeName(string name)
        {
            Name = name;
        }

    }
}
