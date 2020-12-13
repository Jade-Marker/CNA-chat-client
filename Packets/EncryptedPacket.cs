using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

namespace Packets
{
    public class Encryption
    {
        public const int cKeySize = 2048;
        public const int cMaxBytesToEncrypt = 128;
        public const int cEncryptedSize = 256;
    }

    [Serializable]
    public class EncryptedPacket : Packet
    {
        public byte[] data;

        public EncryptedPacket(byte[] data)
        {
            this.data = data;
            packetType = PacketType.ENCRYPTED;
        }

        public static EncryptedPacket EncryptPacket(Packet message, BinaryFormatter formatter, RSAParameters publicKey, RSACryptoServiceProvider rsaProvider)
        {
            MemoryStream memoryStream = new MemoryStream();
            List<byte> encryptedData = new List<byte>();
            formatter.Serialize(memoryStream, message);

            byte[] buffer = memoryStream.GetBuffer();

            int offset = 0;
            while (offset != buffer.Length)
            {
                //Get up to MaxBytesToEncrypt bytes from the buffer
                List<byte> dataPortion = new List<byte>();
                for (int i = 0; i < Encryption.cMaxBytesToEncrypt; i++)
                {
                    if (buffer.Length > offset + i)
                        dataPortion.Add(buffer[i + offset]);
                    else
                        break;
                }
                offset += dataPortion.Count;

                //Then encrypt those bytes
                byte[] data;
                lock (rsaProvider)
                {
                    rsaProvider.ImportParameters(publicKey);
                    data = rsaProvider.Encrypt(dataPortion.ToArray(), true);
                }

                //And add them to our output
                encryptedData.AddRange(data);
            }

            EncryptedPacket packet = new EncryptedPacket(encryptedData.ToArray());
            return packet;
        }

        public static Packet DecryptPacket(EncryptedPacket packet, BinaryFormatter formatter, RSAParameters privateKey, RSACryptoServiceProvider rsaProvider)
        {
            List<byte> decryptedData = new List<byte>();
            int offset = 0;
            while (offset != packet.data.Length)
            {
                //Get up to EncryptedSize bytes from the packet
                List<byte> dataPortion = new List<byte>();
                for (int i = 0; i < Encryption.cEncryptedSize; i++)
                {
                    if (packet.data.Length > offset + i)
                        dataPortion.Add(packet.data[i + offset]);
                    else
                        break;
                }
                offset += dataPortion.Count;

                //Decrypt those bytes
                byte[] data;
                lock (rsaProvider)
                {
                    rsaProvider.ImportParameters(privateKey);
                    data = rsaProvider.Decrypt(dataPortion.ToArray(), true);
                }

                //And add them to our output
                decryptedData.AddRange(data);
            }

            //Data received has now been decrypted, so it can now be deserialised
            MemoryStream memoryStream = new MemoryStream(decryptedData.ToArray());

            Packet decryptedPacket = formatter.Deserialize(memoryStream) as Packet;
            return decryptedPacket;
        }
    }
}

