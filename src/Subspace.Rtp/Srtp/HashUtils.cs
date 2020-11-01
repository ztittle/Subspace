using System.IO;
using System.Security.Cryptography;

namespace Subspace.Rtp.Srtp
{
    internal static class HashUtils
    {
        internal static byte[] HmacSha1(byte[] encrypted, byte[] key)
        {
            var secretKey = new byte[64];

            using var rng = new RNGCryptoServiceProvider();

            // The array is now filled with cryptographically strong random bytes.
            rng.GetBytes(secretKey);

            // Initialize the keyed hash object.
            using var hmac = new HMACSHA1(key);
            using var inStream = new MemoryStream(encrypted);
            using var outStream = new MemoryStream();

            // Compute the hash of the input file.
            var hashValue = hmac.ComputeHash(inStream);
            // Reset inStream to the beginning of the file.
            inStream.Position = 0;
            // Write the computed hash value to the output file.
            outStream.Write(hashValue, 0, hashValue.Length);
            // Copy the contents of the sourceFile to the destFile.
            int bytesRead;
            // read 1K at a time
            var buffer = new byte[1024];
            do
            {
                // Read from the wrapping CryptoStream.
                bytesRead = inStream.Read(buffer, 0, 1024);
                outStream.Write(buffer, 0, bytesRead);
            } while (bytesRead > 0);

            return outStream.GetBuffer();
        }
    }
}
