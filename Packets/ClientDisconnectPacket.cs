using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packets
{
    [Serializable]
    public class ClientDisconnectPacket : Packet
    {
        public string name;

        public ClientDisconnectPacket(string name)
        {
            this.name = name;
            packetType = PacketType.CLIENT_DISCONNECT;
        }
    }
}
