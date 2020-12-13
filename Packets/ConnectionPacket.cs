using System;
using System.Security.Cryptography;

namespace Packets
{
    [Serializable]
    public class ConnectionPacket: Packet
    {
        public string name;
        public RSAParameters publicKey;
        public ConnectionPacket(string name, RSAParameters publicKey)
        {
            this.name = name;
            this.publicKey = publicKey;
            packetType = PacketType.CONNECTION_START;
        }
    }
}
