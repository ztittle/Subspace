using System.Collections.Generic;
using System.Linq;

namespace Subspace.Rtsp
{
    public static class KeyValueParser
    {
        private static string[] SplitPair(string pair, char separator)
        {
            var splitIdx = pair.IndexOf(separator);

            if (splitIdx == -1)
            {
                return new[]
                {
                    pair
                };
            }

            return new[]
            {
                pair.Substring(0, splitIdx), 
                pair.Substring(splitIdx + 1)
            };
        }

        public static List<KeyValuePair<string, string>> ParsePairs(string pairs, char separator)
        {
            var keyValues = pairs
                .Split(separator)
                .Select(pair => SplitPair(pair, '='))
                .Select(parts => new KeyValuePair<string, string>(parts[0], parts.Length > 1 ? parts[1].Trim('\"') : string.Empty))
                .ToList();

            return keyValues;
        }
    }
}
