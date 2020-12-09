using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
