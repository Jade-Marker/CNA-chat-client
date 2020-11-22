using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packets
{
    [Serializable]
    public class NamePacket : Packet
    {
        public string name;

        public NamePacket(string name)
        {
            this.name = name;
            packetType = PacketType.CLIENT_NAME;
        }
    }
}
