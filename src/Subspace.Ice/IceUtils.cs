using System;
using System.Security.Cryptography;
using System.Text;

namespace Subspace.Ice
{
    public static class IceUtils
    {
        public const string _iceChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ01234567890+/";

        /// <summary>
        /// ice-ufrag and ice-pwd attributes MUST be chosen randomly at the
        /// beginning of a session.  The ice-ufrag attribute MUST contain at
        /// least 24 bits of randomness, and the ice-pwd attribute MUST contain
        /// at least 128 bits of randomness.  This means that the ice-ufrag
        /// attribute will be at least 4 characters long, and the ice-pwd at
        /// least 22 characters long, since the grammar for these attributes
        /// allows for 6 bits of randomness per character.  The attributes MAY be
        /// longer than 4 and 22 characters, respectively, of course, up to 256
        /// characters.  The upper limit allows for buffer sizing in
        /// implementations.  Its large upper limit allows for increased amounts
        /// of randomness to be added over time.
        /// 
        /// https://tools.ietf.org/html/rfc5245#section-15.4
        /// </summary>
        public static void GenerateIceUsernamePassword(out string username, out string password)
        {
            var usernameSb = new StringBuilder();
            for (var i = 0; i < 4; i++)
            {
                var iceChar = GetRandomIceChar();

                usernameSb.Append(iceChar);
            }

            var passwordSb = new StringBuilder();
            for (var i = 0; i < 22; i++)
            {
                var iceChar = GetRandomIceChar();

                passwordSb.Append(iceChar);
            }

            username = usernameSb.ToString();
            password = passwordSb.ToString();
        }

        public static char GetRandomIceChar()
        {
            var idx = RandomNumberGenerator.GetInt32(0, _iceChars.Length);

            var iceChar = _iceChars[idx];
            return iceChar;
        }

        public static ulong GetRandomTieBreaker()
        {
            var randomBytes = new byte[8];
            RandomNumberGenerator.Fill(randomBytes);
            return BitConverter.ToUInt64(randomBytes);
        }

        /*
         * priority = (2^24)*(type preference) +
                      (2^8)*(local preference) +
                      (2^0)*(256 - component ID)
         */
        public static int GenerateIcePriority(byte typePreference, ushort localPreference, byte componentId)
        {
            return typePreference << 24 | localPreference << 8 | (256 - componentId);
        }
    }
}
