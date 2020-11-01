namespace Subspace.Rtsp
{
    /// <summary>
    /// 
    /// This field is used to set RTP-specific parameters in the PLAY
    /// response.
    /// 
    /// url:
    /// Indicates the stream URL which for which the following RTP
    /// parameters correspond.
    /// 
    /// seq:
    /// Indicates the sequence number of the first packet of the
    /// stream. This allows clients to gracefully deal with packets
    /// when seeking. The client uses this value to differentiate
    /// packets that originated before the seek from packets that
    /// originated after the seek.
    /// 
    /// rtptime:
    /// Indicates the RTP timestamp corresponding to the time value in
    /// the Range response header. (Note: For aggregate control, a
    /// particular stream may not actually generate a packet for the
    /// Range time value returned or implied. Thus, there is no
    /// guarantee that the packet with the sequence number indicated
    /// by seq actually has the timestamp indicated by rtptime.) The
    /// client uses this value to calculate the mapping of RTP time to
    /// NPT.
    /// 
    /// A mapping from RTP timestamps to NTP timestamps (wall clock) is
    /// available via RTCP. However, this information is not sufficient to
    /// generate a mapping from RTP timestamps to NPT. Furthermore, in
    /// order to ensure that this information is available at the necessary
    /// time (immediately at startup or after a seek), and that it is
    /// delivered reliably, this mapping is placed in the RTSP control
    /// channel.
    /// 
    /// In order to compensate for drift for long, uninterrupted
    /// presentations, RTSP clients should additionally map NPT to NTP,
    /// using initial RTCP sender reports to do the mapping, and later
    /// reports to check drift against the mapping.
    /// 
    /// https://tools.ietf.org/html/rfc2326#section-12.33
    /// </summary>
    public class RtpInfo
    {
        /// <summary>
        /// Indicates the stream URL which for which the following RTP
        /// parameters correspond.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Indicates the sequence number of the first packet of the
        /// stream. This allows clients to gracefully deal with packets
        /// when seeking. The client uses this value to differentiate
        /// packets that originated before the seek from packets that
        /// originated after the seek.
        /// </summary>
        public uint Seq { get; set; }

        /// <summary>
        /// Indicates the RTP timestamp corresponding to the time value in
        /// the Range response header. (Note: For aggregate control, a
        /// particular stream may not actually generate a packet for the
        /// Range time value returned or implied. Thus, there is no
        /// guarantee that the packet with the sequence number indicated
        /// by seq actually has the timestamp indicated by rtptime.) The
        /// client uses this value to calculate the mapping of RTP time to
        /// NPT.
        /// </summary>
        public uint RtpTime { get; set; }
    }
}
