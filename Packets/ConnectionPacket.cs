using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packets
{
    [Serializable]
    public class ConnectionPacket: Packet
    {
        public string name;

        public ConnectionPacket(string name)
        {
            this.name = name;
            packetType = PacketType.CONNECTION_START;
        }
    }
}
