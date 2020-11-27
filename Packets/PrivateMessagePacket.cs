using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packets
{
    [Serializable]
    public class PrivateMessagePacket:Packet
    {
        public string name;
        public string message;

        public PrivateMessagePacket(string name, string message)
        {
            this.name = name;
            this.message = message;
            packetType = PacketType.PRIVATE_MESSAGE;
        }
    }
}
