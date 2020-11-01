using System.Collections.Generic;

namespace Subspace.Rtp.Rtcp
{
    /// <summary>
    /// BYE: Goodbye RTCP Packet
    ///
    /// If a BYE packet is received by a mixer, the mixer SHOULD forward the
    /// BYE packet with the SSRC/CSRC identifier(s) unchanged.  If a mixer
    /// shuts down, it SHOULD send a BYE packet listing all contributing
    /// sources it handles, as well as its own SSRC identifier.  Optionally,
    /// the BYE packet MAY include an 8-bit octet count followed by that many
    /// octets of text indicating the reason for leaving, e.g., "camera
    /// malfunction" or "RTP loop detected".  The string has the same
    /// encoding as that described for SDES.  If the string fills the packet
    /// to the next 32-bit boundary, the string is not null terminated.  If
    /// not, the BYE packet MUST be padded with null octets to the next 32-
    /// bit boundary.  This padding is separate from that indicated by the P
    /// bit in the RTCP header.
    /// 
    /// https://tools.ietf.org/html/rfc3550#section-6.6
    /// </summary>
    public class RtcpGoodbyePacket : RtcpPacket
    {
        public const int HeaderLength = 8;
        public RtcpGoodbyePacket()
            : base(RtcpPacketType.Goodbye)
        {
        }
        public override ushort LengthIn32BitWordsMinusOne => (ushort)((HeaderLength + (SynchronizationSources.Count * 4) + Reason.Length) / 4 - 1);

        public List<uint> SynchronizationSources { get; set; }

        public string Reason { get; set; }
    }
}
