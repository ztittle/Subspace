using System.Collections.Generic;
using System.Linq;

namespace Subspace.Rtp.Rtcp
{
    /// <summary>
    /// The SDES packet is a three-level structure composed of a header and
    /// zero or more chunks, each of which is composed of items describing
    /// the source identified in that chunk.
    /// 
    /// https://tools.ietf.org/html/rfc3550#section-6.5
    /// </summary>
    public class RtcpSourceDescriptionPacket : RtcpPacket
    {
        public const int HeaderLength = 4;
        public const int ChunkSSrcLength = 4;
        public const int SourceDescriptionItemHeaderLength = 2;
        public const int EndLength = 1;

        public RtcpSourceDescriptionPacket()
            : base(RtcpPacketType.SourceDescription)
        {
        }

        public override ushort LengthIn32BitWordsMinusOne
        {
            get
            {
                var length = HeaderLength + Chunks.Sum(c =>
                    ChunkSSrcLength +
                    c.Items.Sum(l => SourceDescriptionItemHeaderLength + l.Text.Length) +
                    EndLength);

                if (length % 4 == 0)
                {
                    return (ushort)(length / 4 - 1);
                }

                return (ushort)(length / 4);
            }
        }

        public List<RtcpSourceDescriptionChunk> Chunks { get; set; }
    }
}
