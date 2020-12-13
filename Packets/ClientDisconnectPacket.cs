using System;

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
