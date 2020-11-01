using System;

namespace Subspace.Rtp
{
    internal static class BigEndianUtils
    {
        public static byte[] ShiftLeft(this byte[] input, int count)
        {
            var byteCount = count / 8;

            var bytes = new byte[input.Length + byteCount];

            input.CopyTo(bytes, 0);

            return bytes;
        }

        public static byte[] Xor(this byte[] left, byte[] right)
        {
            var leftLen = left.Length;
            var rightLen = right.Length;
            var maxLen = Math.Max(leftLen, rightLen);
            var xorBytes = new byte[maxLen];

            for (var i = 0; i < maxLen; i++)
            {
                var leftIdx = leftLen - i - 1;
                var rightIdx = rightLen - i - 1;
                var outputIdx = maxLen - i - 1;

                if (leftIdx < 0)
                {
                    xorBytes[outputIdx] = right[rightIdx];
                }
                else if (rightIdx < 0)
                {
                    xorBytes[outputIdx] = left[leftIdx];
                }
                else
                {
                    xorBytes[outputIdx] = (byte)(left[leftIdx] ^ right[rightIdx]);
                }
            }

            return xorBytes;
        }
    }
}
