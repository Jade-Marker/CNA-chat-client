using System;
using System.Collections.Generic;

namespace Packets
{
    [Serializable]
    public class ChatMessagePacket:Packet
    {
        public List<string> messages;
        public int profilePictureIndex;

        public string message { get {
                if (messages.Count >= 1)
                    return messages[0];
                else
                    return "";
            } }

        public ChatMessagePacket(List<string> messages)
        {
            this.messages = messages;
            profilePictureIndex = -1;
            packetType = PacketType.CHAT_MESSAGE;
        }

        public ChatMessagePacket(string message)
        {
            messages = new List<string>();
            messages.Add(message);
            profilePictureIndex = -1;

            packetType = PacketType.CHAT_MESSAGE;
        }

        public ChatMessagePacket(string message, int profilePicture)
        {
            messages = new List<string>();
            messages.Add(message);

            this.profilePictureIndex = profilePicture;

            packetType = PacketType.CHAT_MESSAGE;
        }
    }
}
