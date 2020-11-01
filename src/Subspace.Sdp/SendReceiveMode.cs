namespace Subspace.Sdp
{
    /// <summary>
    /// https://tools.ietf.org/html/rfc4566#section-6
    /// </summary>
    public enum SendReceiveMode
    {
        /// <summary>
        /// This specifies that the tools should be started in send and
        /// receive mode.  This is necessary for interactive conferences
        /// with tools that default to receive-only mode.  It can be either
        /// a session or media-level attribute, and it is not dependent on
        /// charset.
        /// 
        /// If none of the attributes "sendonly", "recvonly", "inactive",
        /// and "sendrecv" is present, "sendrecv" SHOULD be assumed as the
        /// default for sessions that are not of the conference type
        /// "broadcast" or "H332" (see below).
        /// </summary>
        SendRecv,
        /// <summary>
        /// =This specifies that the tools should be started in receive-only
        /// mode where applicable.  It can be either a session- or media-
        /// level attribute, and it is not dependent on charset.  Note that
        /// recvonly applies to the media only, not to any associated
        /// control protocol (e.g., an RTP-based system in recvonly mode
        /// SHOULD still send RTCP packets).
        /// </summary>
        RecvOnly,
        /// <summary>
        /// This specifies that the tools should be started in send-only
        /// mode.  An example may be where a different unicast address is
        /// to be used for a traffic destination than for a traffic source.
        /// In such a case, two media descriptions may be used, one
        /// sendonly and one recvonly.  It can be either a session- or
        /// media-level attribute, but would normally only be used as a
        /// media attribute.  It is not dependent on charset.  Note that
        /// sendonly applies only to the media, and any associated control
        /// protocol (e.g., RTCP) SHOULD still be received and processed as
        /// normal.
        /// </summary>
        SendOnly,
        /// <summary>
        /// This specifies that the tools should be started in inactive
        /// mode.  This is necessary for interactive conferences where
        /// users can put other users on hold.  No media is sent over an
        /// inactive media stream.  Note that an RTP-based system SHOULD
        /// still send RTCP, even if started inactive.  It can be either a
        /// session or media-level attribute, and it is not dependent on
        /// charset.
        /// </summary>
        Inactive
    }

    public static class SendReceiveModeExtensions
    {
        public static string ToSdpValue(this SendReceiveMode sendReceiveMode)
        {
            return sendReceiveMode switch
            {
                SendReceiveMode.SendRecv => "sendrecv",
                SendReceiveMode.SendOnly => "sendonly",
                SendReceiveMode.RecvOnly => "recvonly",
                SendReceiveMode.Inactive => "inactive",
                _ => null
            };
        }
    }
}
