using System.Collections.Generic;

namespace Subspace.Rtp.Rtcp
{
    /// <summary>
    /// The sender report packet consists of three sections, possibly
    /// followed by a fourth profile-specific extension section if defined.
    /// The first section, the header, is 8 octets long.
    ///
    /// https://tools.ietf.org/html/rfc3550#section-6.4.1
    /// </summary>
    public class RtcpSenderReportPacket : RtcpPacket
    {
        public const int HeaderLength = 8;
        public const int SenderInfoLength = 20;
        public const int ReportBlockLength = 24;

        public RtcpSenderReportPacket()
            : base(RtcpPacketType.SenderReport)
        {
        }

        public override ushort LengthIn32BitWordsMinusOne => (ushort)((HeaderLength + SenderInfoLength + ReportBlockLength * ReceptionReports.Count) / 4 - 1);

        /// <summary>
        /// SSRC: 32 bits
        /// 
        /// The synchronization source identifier for the originator of this
        /// SR packet.
        /// </summary>
        public uint SynchronizationSource { get; set; }

        /// <summary>
        /// NTP timestamp: 64 bits
        /// 
        /// Indicates the wallclock time (see Section 4) when this report was
        /// sent so that it may be used in combination with timestamps
        /// returned in reception reports from other receivers to measure
        /// round-trip propagation to those receivers.  Receivers should
        /// expect that the measurement accuracy of the timestamp may be
        /// limited to far less than the resolution of the NTP timestamp.  The
        /// measurement uncertainty of the timestamp is not indicated as it
        /// may not be known.  On a system that has no notion of wallclock
        /// time but does have some system-specific clock such as "system
        /// uptime", a sender MAY use that clock as a reference to calculate
        /// relative NTP timestamps.  It is important to choose a commonly
        /// used clock so that if separate implementations are used to produce
        /// the individual streams of a multimedia session, all
        /// implementations will use the same clock.  Until the year 2036,
        /// relative and absolute timestamps will differ in the high bit so
        /// (invalid) comparisons will show a large difference; by then one
        /// hopes relative timestamps will no longer be needed.  A sender that
        /// has no notion of wallclock or elapsed time MAY set the NTP
        /// timestamp to zero.
        /// 
        /// https://tools.ietf.org/html/rfc3550#section-6.4.1
        /// </summary>
        public ulong NtpTimestamp { get; set; }

        /// <summary>
        /// RTP timestamp: 32 bits
        /// 
        /// Corresponds to the same time as the NTP timestamp (above), but in
        /// the same units and with the same random offset as the RTP
        /// timestamps in data packets.  This correspondence may be used for
        /// intra- and inter-media synchronization for sources whose NTP
        /// timestamps are synchronized, and may be used by media-independent
        /// receivers to estimate the nominal RTP clock frequency.  Note that
        /// in most cases this timestamp will not be equal to the RTP
        /// timestamp in any adjacent data packet.  Rather, it MUST be
        /// calculated from the corresponding NTP timestamp using the
        /// relationship between the RTP timestamp counter and real time as
        /// maintained by periodically checking the wallclock time at a
        /// sampling instant.
        /// 
        /// https://tools.ietf.org/html/rfc3550#section-6.4.1
        /// </summary>
        public uint RtpTimestamp { get; set; }

        /// <summary>
        /// sender's packet count: 32 bits
        /// 
        /// The total number of RTP data packets transmitted by the sender
        /// since starting transmission up until the time this SR packet was
        /// generated.  The count SHOULD be reset if the sender changes its
        /// SSRC identifier.
        ///
        /// https://tools.ietf.org/html/rfc3550#section-6.4.1
        /// </summary>
        public uint SenderPacketCount { get; set; }

        /// <summary>
        /// sender's octet count: 32 bits
        /// 
        /// The total number of payload octets (i.e., not including header or
        /// padding) transmitted in RTP data packets by the sender since
        /// starting transmission up until the time this SR packet was
        /// generated.  The count SHOULD be reset if the sender changes its
        /// SSRC identifier.  This field can be used to estimate the average
        /// payload data rate.
        /// 
        /// https://tools.ietf.org/html/rfc3550#section-6.4.1
        /// </summary>
        public uint SenderOctetCount { get; set; }

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