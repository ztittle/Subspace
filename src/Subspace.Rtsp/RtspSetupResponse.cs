namespace Subspace.Rtsp
{
    public class RtspSetupResponse
    {
        /// <summary>
        /// This request and response header field identifies an RTSP session
        /// started by the media server in a SETUP response and concluded by
        /// TEARDOWN on the presentation URL. The session identifier is chosen by
        /// the media server (see Section 3.4). Once a client receives a Session
        /// identifier, it MUST return it for any request related to that
        /// session.  A server does not have to set up a session identifier if it
        /// has other means of identifying a session, such as dynamically
        /// generated URLs.
        /// 
        /// https://tools.ietf.org/html/rfc2326#section-12.37
        /// </summary>
        public string Session { get; internal set; }

        /// <summary>
        /// The timeout parameter is only allowed in a response header. The
        /// server uses it to indicate to the client how long the server is
        /// prepared to wait between RTSP commands before closing the session due
        /// to lack of activity (see Section A). The timeout is measured in
        /// seconds, with a default of 60 seconds (1 minute).
        ///
        /// https://tools.ietf.org/html/rfc2326#section-12.37
        /// </summary>
        public int SessionTimeoutSeconds { get; set; }

        /// <summary>
        /// This parameter provides the unicast RTP/RTCP port pair on
        /// which the server has chosen to receive media data and control
        /// information.  It is specified as a range, e.g.,
        /// server_port=3456-3457.
        /// 
        /// https://tools.ietf.org/html/rfc2326#section-12.39
        /// </summary>
        public int[] ServerPorts { get; internal set; }

        /// <summary>
        /// The ssrc parameter indicates the RTP SSRC [24, Sec. 3] value
        /// that should be (request) or will be (response) used by the
        /// media server. This parameter is only valid for unicast
        /// transmission. It identifies the synchronization source to be
        /// associated with the media stream.
        /// 
        /// https://tools.ietf.org/html/rfc2326#section-12.39
        /// </summary>
        public uint Ssrc { get; internal set; }
        public RtspResponseMessage ResponseMessage { get; internal set; }
    }
}
