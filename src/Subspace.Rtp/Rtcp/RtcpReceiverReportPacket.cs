using System.Collections.Generic;

namespace Subspace.Rtp.Rtcp
{
    /// <summary>
    /// The format of the receiver report (RR) packet is the same as that of
    /// the SR packet except that the packet type field contains the constant
    /// 201 and the five words of sender information are omitted (these are
    /// the NTP and RTP timestamps and sender's packet and octet counts).
    /// The remaining fields have the same meaning as for the SR packet.
    /// 
    /// An empty RR packet (RC = 0) MUST be put at the head of a compound
    /// RTCP packet when there is no data transmission or reception to
    /// report.
    /// 
    /// https://tools.ietf.org/html/rfc3550#section-6.4.2
    /// </summary>
    public class RtcpReceiverReportPacket : RtcpPacket
    {
        public const int HeaderLength = 8;
        public const int ReportBlockLength = 24;

        public RtcpReceiverReportPacket()
            : base(RtcpPacketType.ReceiverReport)
        {
        }

        public override ushort LengthIn32BitWordsMinusOne => (ushort)((HeaderLength + ReportBlockLength * ReceptionReports.Count) / 4 - 1);

        /// <summary>
        /// SSRC: 32 bits
        /// 
        /// The synchronization source identifier for the originator of this
        /// SR packet.
        /// </summary>
        public uint SynchronizationSource { get; set; }

        /// <summary>
        /// The third section contains zero or more reception report blocks
        /// depending on the number of other sources heard by this sender since
        /// the last report.  Each reception report block conveys statistics on
        /// the reception of RTP packets from a single synchronization source.
        /// Receivers SHOULD NOT carry over statistics when a source changes its
        /// SSRC identifier due to a collision.
        /// 
        /// https://tools.ietf.org/html/rfc3550#section-6.4.1
        /// </summary>
        public List<RtcpReceptionReport> ReceptionReports { get; set; }
    }
}