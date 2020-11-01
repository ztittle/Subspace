using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Subspace.Rtsp
{
    public static class HashUtils
    {
        private static readonly uint[] _lookup32 = CreateLookup32();
        private static readonly CultureInfo _ic = CultureInfo.InvariantCulture;

        private static uint[] CreateLookup32()
        {
            var result = new uint[256];

            for (var i = 0; i < 256; i++)
            {
                var s = i.ToString("x2", _ic);
                result[i] = s[0] + ((uint)s[1] << 16);
            }

            return result;
        }

        // https://stackoverflow.com/questions/311165/how-do-you-convert-a-byte-array-to-a-hexadecimal-string-and-vice-versa/24343727#24343727
        private static string ToHexString(this byte[] bytes)
        {
            var len = bytes.Length;
            var result = new char[len * 2];
            for (var i = 0; i < len; i++)
            {
                var val = _lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 16);
            }
            return new string(result);
        }

        internal static string ComputeHashString(this MD5 md5, string value)
        {
            return md5.ComputeHash(Encoding.ASCII.GetBytes(value)).ToHexString();
        }
    }
}
