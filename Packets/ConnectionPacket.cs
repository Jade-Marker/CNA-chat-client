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
        public RSAParameters PublicKey;
        public ConnectionPacket(string name, RSAParameters PublicKey)
        {
            this.name = name;
            this.PublicKey = PublicKey;
            packetType = PacketType.CONNECTION_START;
        }
    }
}
