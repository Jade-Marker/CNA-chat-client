using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Packets
{
    [Serializable]
    public class EncryptedPacket : Packet
    {
        public byte[] data;

        public EncryptedPacket(byte[] data)
        {
            this.data = data;
            packetType = PacketType.ENCRYPTED;
        }

        public static EncryptedPacket EncryptPacket(Packet message, BinaryFormatter formatter, RSAParameters publicKey, RSACryptoServiceProvider RSAProvider)
        {
            MemoryStream memoryStream = new MemoryStream();
            List<byte> encryptedData = new List<byte>();
            formatter.Serialize(memoryStream, message);

            byte[] buffer = memoryStream.GetBuffer();

            int offset = 0;
            while (offset != buffer.Length)
            {
                List<byte> dataPortion = new List<byte>();
                for (int i = 0; i < Encryption.MaxBytesToEncrypt; i++)
                {
                    if (buffer.Length > offset + i)
                        dataPortion.Add(buffer[i + offset]);
                    else
                        break;
                }
                offset += dataPortion.Count;

                byte[] data;
                lock (RSAProvider)
                {
                    RSAProvider.ImportParameters(publicKey);
                    data = RSAProvider.Encrypt(dataPortion.ToArray(), true);
                }

                encryptedData.AddRange(data);
            }

            EncryptedPacket packet = new EncryptedPacket(encryptedData.ToArray());
            return packet;
        }

        public static Packet DecryptPacket(EncryptedPacket packet, BinaryFormatter formatter, RSAParameters privateKey, RSACryptoServiceProvider RSAProvider)
        {
            List<byte> decryptedData = new List<byte>();
            int offset = 0;
            while (offset != packet.data.Length)
            {
                List<byte> dataPortion = new List<byte>();
                for (int i = 0; i < Encryption.EncryptedSize; i++)
                {
                    if (packet.data.Length > offset + i)
                        dataPortion.Add(packet.data[i + offset]);
                    else
                        break;
                }
                offset += dataPortion.Count;

                byte[] data;
                lock (RSAProvider)
                {
                    RSAProvider.ImportParameters(privateKey);
                    data = RSAProvider.Decrypt(dataPortion.ToArray(), true);
                }

                decryptedData.AddRange(data);
            }


            MemoryStream memoryStream = new MemoryStream(decryptedData.ToArray());

            Packet decryptedPacket = formatter.Deserialize(memoryStream) as Packet;
            return decryptedPacket;
        }
    }

    public class Encryption
    {
        public static int KeySize = 2048;
        public static int MaxBytesToEncrypt = 128;
        public static int EncryptedSize = 256;
    }
}

