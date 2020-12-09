﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
