using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packets
{
    [Serializable]
    public class ChatMessagePacket:Packet
    {
        public List<string> messages;
        public string message { get {
                if (messages.Count >= 1)
                    return messages[0];
                else
                    return "";
            } }

        public ChatMessagePacket(List<string> messages)
        {
            this.messages = messages;
            packetType = PacketType.CHAT_MESSAGE;
        }

        public ChatMessagePacket(string message)
        {
            messages = new List<string>();
            messages.Add(message);
            packetType = PacketType.CHAT_MESSAGE;
        }
    }
}
