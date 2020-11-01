using System;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;

namespace Subspace.Rtp.Srtp
{
    /// <summary>
    ///
    /// 
    /// https://web.archive.org/web/20041104235618/http://csrc.nist.gov/CryptoToolkit/modes/workshop1/papers/lipmaa-ctr.pdf
    /// https://tools.ietf.org/html/rfc3711#section-4.1.1
    /// </summary>
    public class AesCounterMode
    {
        public static byte[] Encrypt(byte[] key, byte[] iv, byte[] input)
        {
            var count = (input.Length / 32 + 2) * 32;

            var encryptedCounter = GenerateKeystreamSegment(key, iv, count);

            return (new BigInteger(encryptedCounter) ^ new BigInteger(input)).ToByteArray();
        }

        /// <summary>
        /// Conceptually, counter mode [AES-CTR] consists of encrypting
        /// successive integers.  The actual definition is somewhat more
        /// complicated, in order to randomize the starting point of the integer
        /// sequence.  Each packet is encrypted with a distinct keystream
        /// segment, which SHALL be computed as follows.
        /// 
        /// A keystream segment SHALL be the concatenation of the 128-bit output
        /// blocks of the AES cipher in the encrypt direction, using key k = k_e,
        /// in which the block indices are in increasing order.  Symbolically,
        /// each keystream segment looks like
        /// 
        /// E(k, IV) || E(k, IV + 1 mod 2^128) || E(k, IV + 2 mod 2^128) ...
        ///
        /// where the 128-bit integer value IV SHALL be defined by the SSRC, the
        /// SRTP packet index i, and the SRTP session salting key k_s, as below.
        /// 
        /// 
        /// https://tools.ietf.org/html/rfc3711#section-4.1.1
        /// </summary>
        public static byte[] GenerateKeystreamSegment(byte[] key, byte[] iv, int count)
        {
            const int blockSize = 128 / 8;
            var ivInputBlock = new byte[blockSize];
            iv.CopyTo(ivInputBlock, 0);

            var aes = Aes.Create();
            aes.Mode = CipherMode.ECB;
            aes.Key = key;

            var remainingBytesToFillBlock = (blockSize - count % blockSize) % blockSize;

            const int finalBlockSize = blockSize;

            var output = new byte[count + remainingBytesToFillBlock + finalBlockSize];

            var numberOfBlocks = (output.Length - finalBlockSize) / blockSize;

            var counter = 0;

            var cryptoTransform = aes.CreateEncryptor();
            using (var memoryStream = new MemoryStream(output))
            using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
            {
                while (counter < numberOfBlocks)
                {
                    var counterBytes = BitConverter.GetBytes(counter);
                    ivInputBlock[14] = counterBytes[1];
                    ivInputBlock[15] = counterBytes[0];

                    cryptoStream.Write(ivInputBlock);

                    counter++;
                }
            }

            return output.AsSpan(0, count).ToArray();
        }
    }
}
