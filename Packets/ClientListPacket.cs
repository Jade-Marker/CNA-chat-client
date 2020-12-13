using System;
using System.Collections.Generic;

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
