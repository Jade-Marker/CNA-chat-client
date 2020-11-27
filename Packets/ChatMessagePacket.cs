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
        public string message;

        public ChatMessagePacket(string message)
        {
            this.message = message;
            packetType = PacketType.CHAT_MESSAGE;
        }
    }
}
