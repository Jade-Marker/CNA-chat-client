using System;

namespace Packets
{
    [Serializable]
    public class ClientConnectPacket:Packet
    {
        public string name;

        public ClientConnectPacket(string name)
        {
            this.name = name;
            packetType = PacketType.CLIENT_CONNECT;
        }
    }
}
