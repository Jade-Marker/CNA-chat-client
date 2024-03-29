﻿using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Packets
{
    public enum PacketType
    {
        EMPTY,
        CHAT_MESSAGE,
        PRIVATE_MESSAGE,
        CONNECTION_START,
        CLIENT_LIST,
        CLIENT_CONNECT,
        CLIENT_DISCONNECT,
        ENCRYPTED,
        SERVER_KEY
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
