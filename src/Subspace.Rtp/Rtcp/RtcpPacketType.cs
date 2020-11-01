namespace Subspace.Rtp.Rtcp
{
    public enum RtcpPacketType : byte
    {
        /// <summary>
        /// RTP receivers provide reception quality feedback using RTCP report
        /// packets which may take one of two forms depending upon whether or not
        /// the receiver is also a sender.  The only difference between the
        /// sender report (SR) and receiver report (RR) forms, besides the packet
        /// type code, is that the sender report includes a 20-byte sender
        /// information section for use by active senders.  The SR is issued if a
        /// site has sent any data packets during the interval since issuing the
        /// last report or the previous one, otherwise the RR is issued. 
        /// 
        /// https://tools.ietf.org/html/rfc3550#section-6.4
        /// </summary>
        SenderReport = 200,

        /// <summary>
        /// The format of the receiver report (RR) packet is the same as that of
        /// the SR packet except that the packet type field contains the constant
        /// 201 and the five words of sender information are omitted (these are
        /// the NTP and RTP timestamps and sender's packet and octet counts).
        /// The remaining fields have the same meaning as for the SR packet.
        ///
        /// https://tools.ietf.org/html/rfc3550#section-6.4.2
        /// </summary>
        ReceiverReport = 201,

        /// <summary>
        /// The SDES packet is a three-level structure composed of a header and
        /// zero or more chunks, each of which is composed of items describing
        /// the source identified in that chunk.  The items are described
        /// individually in subsequent sections.
        ///
        /// https://tools.ietf.org/html/rfc3550#section-6.5
        /// </summary>
        SourceDescription = 202,

        /// <summary>
        /// The BYE packet indicates that one or more sources are no longer
        /// active.
        ///
        /// https://tools.ietf.org/html/rfc3550#section-6.6
        /// </summary>
        Goodbye = 203,

        /// <summary>
        ///     The APP packet is intended for experimental use as new applications
        /// and new features are developed, without requiring packet type value
        /// registration.  APP packets with unrecognized names SHOULD be ignored.
        /// After testing and if wider use is justified, it is RECOMMENDED that
        /// each APP packet be redefined without the subtype and name fields and
        /// registered with IANA using an RTCP packet type.
        ///
        /// https://tools.ietf.org/html/rfc3550#section-6.7
        /// </summary>
        ApplicationDefined = 204
    }
}