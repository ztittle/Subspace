using System.Collections.Generic;

namespace Subspace.Sdp
{
    /// <summary>
    /// This attribute maps from an RTP payload type number (as used in
    /// an "m=" line) to an encoding name denoting the payload format
    /// to be used.  It also provides information on the clock rate and
    /// encoding parameters.  It is a media-level attribute that is not
    /// dependent on charset.
    /// 
    /// Although an RTP profile may make static assignments of payload
    /// type numbers to payload formats, it is more common for that
    /// assignment to be done dynamically using "a=rtpmap:" attributes.
    /// As an example of a static payload type, consider u-law PCM
    /// coded single-channel audio sampled at 8 kHz.  This is
    /// completely defined in the RTP Audio/Video profile as payload
    /// type 0, so there is no need for an "a=rtpmap:" attribute, and
    /// the media for such a stream sent to UDP port 49232 can be
    /// specified as:
    ///
    ///       m=audio 49232 RTP/AVP 0
    /// 
    /// An example of a dynamic payload type is 16-bit linear encoded
    /// stereo audio sampled at 16 kHz.  If we wish to use the dynamic
    /// RTP/AVP payload type 98 for this stream, additional information
    /// is required to decode it:
    /// 
    /// m=audio 49232 RTP/AVP 98
    /// a=rtpmap:98 L16/16000/2
    /// 
    /// Up to one rtpmap attribute can be defined for each media format
    /// specified.  Thus, we might have the following:
    /// 
    /// m=audio 49230 RTP/AVP 96 97 98
    /// a=rtpmap:96 L8/8000
    /// a=rtpmap:97 L16/8000
    /// a=rtpmap:98 L16/11025/2
    /// 
    /// RTP profiles that specify the use of dynamic payload types MUST
    /// define the set of valid encoding names and/or a means to
    /// register encoding names if that profile is to be used with SDP.
    /// The "RTP/AVP" and "RTP/SAVP" profiles use media subtypes for
    /// encoding names, under the top-level media type denoted in the
    /// "m=" line.  In the example above, the media types are
    /// "audio/l8" and "audio/l16".
    /// 
    /// For audio streams, &lt;encoding parameters&gt; indicates the number
    /// of audio channels.  This parameter is OPTIONAL and may be
    /// omitted if the number of channels is one, provided that no
    /// additional parameters are needed.
    /// 
    /// For video streams, no encoding parameters are currently
    /// specified.
    /// 
    /// Additional encoding parameters MAY be defined in the future,
    /// but codec-specific parameters SHOULD NOT be added.  Parameters
    /// added to an "a=rtpmap:" attribute SHOULD only be those required
    /// for a session directory to make the choice of appropriate media
    /// to participate in a session.  Codec-specific parameters should
    /// be added in other attributes (for example, "a=fmtp:").
    /// 
    /// Note: RTP audio formats typically do not include information
    /// about the number of samples per packet.  If a non-default (as
    /// defined in the RTP Audio/Video Profile) packetisation is
    /// required, the "ptime" attribute is used as given above.
    /// 
    /// https://tools.ietf.org/html/rfc4566#section-6
    /// </summary>
    public class SdpMediaFormatDescription
    {
        /// <summary>
        /// https://www.iana.org/assignments/rtp-parameters/rtp-parameters.xhtml
        /// </summary>
        public int PayloadType { get; set; }
        public string EncodingName { get; set; }
        public int ClockRate { get; set; }
        public string EncodingParameters { get; set; }

        /// <summary>
        /// a=fmtp:&lt;format&gt; &lt;format specific parameters&gt;
        /// 
        /// This attribute allows parameters that are specific to a
        /// particular format to be conveyed in a way that SDP does not
        /// have to understand them.  The format must be one of the formats
        /// specified for the media.  Format-specific parameters may be any
        /// set of parameters required to be conveyed by SDP and given
        /// unchanged to the media tool that will use this format.  At most
        /// one instance of this attribute is allowed for each format.
        /// 
        /// It is a media-level attribute, and it is not dependent on
        /// charset.
        /// </summary>
        public string FormatParameters { get; set; }

        /// <summary>
        ///    A new payload format-specific SDP attribute is defined to indicate
        /// the capability of using RTCP feedback as specified in this document:
        /// "a=rtcp-fb".  The "rtcp-fb" attribute MUST only be used as an SDP
        /// media attribute and MUST NOT be provided at the session level.  The
        /// "rtcp-fb" attribute MUST only be used in media sessions for which the
        /// "AVPF" is specified.
        /// 
        /// The "rtcp-fb" attribute SHOULD be used to indicate which RTCP FB
        /// messages MAY be used in this media session for the indicated payload
        /// type.  A wildcard payload type ("*") MAY be used to indicate that the
        /// RTCP feedback attribute applies to all payload types.  If several
        /// types of feedback are supported and/or the same feedback shall be
        /// specified for a subset of the payload types, several "a=rtcp-fb"
        /// lines MUST be used.
        /// https://tools.ietf.org/html/rfc4585#section-4.2
        /// </summary>
        public List<string> RtcpFeedbackCapability { get; set; } = new List<string>();
    }
}
