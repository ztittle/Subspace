namespace Subspace.Rtp.Rtcp
{
    /// <summary>
    /// Conveys statistics on
    /// the reception of RTP packets from a single synchronization source.
    /// Receivers SHOULD NOT carry over statistics when a source changes its
    /// SSRC identifier due to a collision.
    /// 
    /// https://tools.ietf.org/html/rfc3550#section-6.4.1
    /// </summary>
    public class RtcpReceptionReport
    {
        /// <summary>
        /// SSRC_n (source identifier): 32 bits
        /// 
        /// The SSRC identifier of the source to which the information in this
        /// reception report block pertains.
        /// 
        /// https://tools.ietf.org/html/rfc3550#section-6.4.1
        /// </summary>
        public uint SynchronizationSource { get; set; }

        /// <summary>
        /// fraction lost: 8 bits
        /// 
        /// The fraction of RTP data packets from source SSRC_n lost since the
        /// previous SR or RR packet was sent, expressed as a fixed point
        /// number with the binary point at the left edge of the field.  (That
        /// is equivalent to taking the integer part after multiplying the
        /// loss fraction by 256.)  This fraction is defined to be the number
        /// of packets lost divided by the number of packets expected, as
        /// defined in the next paragraph.  An implementation is shown in
        /// Appendix A.3.  If the loss is negative due to duplicates, the
        /// fraction lost is set to zero.  Note that a receiver cannot tell
        /// whether any packets were lost after the last one received, and
        /// that there will be no reception report block issued for a source
        /// if all packets from that source sent during the last reporting
        /// interval have been lost.
        /// 
        /// https://tools.ietf.org/html/rfc3550#section-6.4.1
        /// </summary>
        public byte FractionLost { get; set; }

        /// <summary>
        /// cumulative number of packets lost: 24 bits
        /// 
        /// The total number of RTP data packets from source SSRC_n that have
        /// been lost since the beginning of reception.  This number is
        /// defined to be the number of packets expected less the number of
        /// packets actually received, where the number of packets received
        /// includes any which are late or duplicates.  Thus, packets that
        /// arrive late are not counted as lost, and the loss may be negative
        /// if there are duplicates.  The number of packets expected is
        /// defined to be the extended last sequence number received, as
        /// defined next, less the initial sequence number received.  This may
        /// be calculated as shown in Appendix A.3.
        /// 
        /// https://tools.ietf.org/html/rfc3550#section-6.4.1
        /// </summary>
        public uint CumulativeNumberOfPacketsLost { get; set; }

        /// <summary>
        /// extended highest sequence number received: 32 bits
        /// 
        /// The low 16 bits contain the highest sequence number received in an
        /// RTP data packet from source SSRC_n, and the most significant 16
        /// bits extend that sequence number with the corresponding count of
        /// sequence number cycles, which may be maintained according to the
        /// algorithm in Appendix A.1.  Note that different receivers within
        /// the same session will generate different extensions to the
        /// sequence number if their start times differ significantly.
        /// 
        /// https://tools.ietf.org/html/rfc3550#section-6.4.1
        /// </summary>
        public uint ExtendedHighestSequenceNumberReceived { get; set; }

        /// <summary>
        /// interarrival jitter: 32 bits
        /// 
        /// An estimate of the statistical variance of the RTP data packet
        /// interarrival time, measured in timestamp units and expressed as an
        /// unsigned integer.  The interarrival jitter J is defined to be the
        /// mean deviation (smoothed absolute value) of the difference D in
        /// packet spacing at the receiver compared to the sender for a pair
        /// of packets.  As shown in the equation below, this is equivalent to
        /// the difference in the "relative transit time" for the two packets;
        /// the relative transit time is the difference between a packet's RTP
        /// timestamp and the receiver's clock at the time of arrival,
        /// measured in the same units.
        ///
        /// 
        /// https://tools.ietf.org/html/rfc3550#section-6.4.1
        /// </summary>
        public int InterarrivalJitter { get; set; }

        /// <summary>
        /// last SR timestamp (LSR): 32 bits
        /// 
        /// The middle 32 bits out of 64 in the NTP timestamp (as explained in
        /// Section 4) received as part of the most recent RTCP sender report
        /// (SR) packet from source SSRC_n.  If no SR has been received yet,
        /// the field is set to zero.
        ///
        /// https://tools.ietf.org/html/rfc3550#section-6.5
        /// </summary>
        public uint LastSRTimestamp { get; set; }

        /// <summary>
        /// delay since last SR (DLSR): 32 bits
        /// 
        /// The delay, expressed in units of 1/65536 seconds, between
        /// receiving the last SR packet from source SSRC_n and sending this
        /// reception report block.  If no SR packet has been received yet
        /// from SSRC_n, the DLSR field is set to zero.
        /// 
        /// Let SSRC_r denote the receiver issuing this receiver report.
        /// Source SSRC_n can compute the round-trip propagation delay to
        /// SSRC_r by recording the time A when this reception report block is
        /// received.  It calculates the total round-trip time A-LSR using the
        /// last SR timestamp (LSR) field, and then subtracting this field to
        /// leave the round-trip propagation delay as (A - LSR - DLSR).  This
        /// is illustrated in Fig. 2.  Times are shown in both a hexadecimal
        /// representation of the 32-bit fields and the equivalent floating-
        /// point decimal representation.  Colons indicate a 32-bit field
        /// divided into a 16-bit integer part and 16-bit fraction part.
        /// 
        /// This may be used as an approximate measure of distance to cluster
        /// receivers, although some links have very asymmetric delays.
        ///
        /// https://tools.ietf.org/html/rfc3550#section-6.5
        /// </summary>
        public uint DelaySinceLastSR { get; set; }
    }
}