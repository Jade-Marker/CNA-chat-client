using System;
using System.Security.Cryptography;

namespace Packets
{
    [Serializable]
    public class ServerKeyPacket:Packet
    {
        public RSAParameters publicKey;

        public ServerKeyPacket(RSAParameters publicKey)
        {
            this.publicKey = publicKey;
            packetType = PacketType.SERVER_KEY;
        }
    }
}
