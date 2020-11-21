using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Packets
{
    public enum PacketType {
        EMPTY,
        CHAT_MESSAGE,
        PRIVATE_MESSAGE,
        CLIENT_NAME
    };

    [Serializable]
    public class Packet
    {
        public PacketType packetType { get; protected set; }

        public static Packet ReadPacket(BinaryReader reader, BinaryFormatter formatter)
        {
            int numberOfBytes;
            if ((numberOfBytes = reader.ReadInt32()) != -1)
            {
                byte[] buffer = reader.ReadBytes(numberOfBytes);
                MemoryStream memoryStream = new MemoryStream(buffer);

                return formatter.Deserialize(memoryStream) as Packet;
            }
            else
                return null;
        }

        public static void SendPacket(Packet message, BinaryFormatter formatter, BinaryWriter writer)
        {
            MemoryStream memoryStream = new MemoryStream();
            formatter.Serialize(memoryStream, message);

            byte[] buffer = memoryStream.GetBuffer();
            writer.Write(buffer.Length);
            writer.Write(buffer);
            writer.Flush();
        }
    }
}
