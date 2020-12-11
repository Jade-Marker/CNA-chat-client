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
        public int profilePictureIndex;

        public PrivateMessagePacket(string name, string message, int profilePictureIndex)
        {
            this.name = name;
            this.message = message;
            this.profilePictureIndex = profilePictureIndex;
            packetType = PacketType.PRIVATE_MESSAGE;
        }
    }
}
