using System.Net;

namespace Subspace.Rtp.Rtcp
{
    /// <summary>
    /// https://tools.ietf.org/html/rfc3550#section-6.1
    /// </summary>
    public abstract class RtcpPacket
    {
        public RtcpPacket(RtcpPacketType packetType)
        {
            PacketType = packetType;
        }

        public IPEndPoint RemoteEndPoint { get; set; }

        /// <summary>
        /// version (V): 2 bits
        /// 
        /// Identifies the version of RTP, which is the same in RTCP packets
        /// as in RTP data packets.  The version defined by this specification
        /// is two (2).
        /// </summary>
        public byte Version { get; } = 2;

        /// <summary>
        /// padding (P): 1 bit
        /// 
        /// If the padding bit is set, this individual RTCP packet contains
        /// some additional padding octets at the end which are not part of
        /// the control information but are included in the length field.  The
        /// last octet of the padding is a count of how many padding octets
        /// should be ignored, including itself (it will be a multiple of
        /// four).  Padding may be needed by some encryption algorithms with
        /// fixed block sizes.  In a compound RTCP packet, padding is only
        /// required on one individual packet because the compound packet is
        /// encrypted as a whole for the method in Section 9.1.  Thus, padding
        /// MUST only be added to the last individual packet, and if padding
        /// is added to that packet, the padding bit MUST be set only on that
        /// packet.  This convention aids the header validity checks described
        /// in Appendix A.2 and allows detection of packets from some early
        /// implementations that incorrectly set the padding bit on the first
        /// individual packet and add padding to the last individual packet.
        /// </summary>
        public bool Padding { get; set; }

        /// <summary>
        /// packet type (PT): 8 bits
        /// </summary>
        public RtcpPacketType PacketType { get; }

        /// <summary>
        /// length: 16 bits
        /// 
        /// The length of this RTCP packet in 32-bit words minus one,
        /// including the header and any padding.  (The offset of one makes
        /// zero a valid length and avoids a possible infinite loop in
        /// scanning a compound RTCP packet, while counting 32-bit words
        /// avoids a validity check for a multiple of 4.)
        /// </summary>
        public abstract ushort LengthIn32BitWordsMinusOne { get; }
    }
}
