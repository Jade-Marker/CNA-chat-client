using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packets
{
    [Serializable]
    public class ClientListPacket: Packet
    {
        public List<string> clientNames;

        public ClientListPacket(List<string> names)
        {
            clientNames = names;
            packetType = PacketType.CLIENT_LIST;
        }
    }
}
