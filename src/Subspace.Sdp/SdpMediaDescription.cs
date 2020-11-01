using System;
using System.Collections.Generic;

namespace Subspace.Sdp
{
    /// <summary>
    /// https://tools.ietf.org/html/rfc4566#section-5.14
    ///
    /// m=&lt;media&gt; &lt;port&gt; &lt;proto&gt; &lt;fmt&gt; ...
    /// 
    /// A session description may contain a number of media descriptions.
    /// Each media description starts with an "m=" field and is terminated by
    /// either the next "m=" field or by the end of the session description.
    /// A media field has several sub-fields:
    /// 
    /// </summary>
    public class SdpMediaDescription
    {
        /// <summary>
        /// Used for identifying media streams within a
        /// session description.
        /// 
        /// https://tools.ietf.org/html/rfc5888#section-4
        /// </summary>
        public string MediaId { get; set; }

        /// <summary>
        /// &lt;media&gt; is the media type.  Currently defined media are "audio",
        /// "video", "text", "application", and "message", although this list
        /// may be extended in the future (see Section 8).
        /// </summary>
        public string Media { get; set; }

        /// <summary>
        /// The Real-time Transport Protocol (RTP) [1] comprises two components:
        /// a data transfer protocol and an associated control protocol (RTCP).
        /// Historically, RTP and RTCP have been run on separate UDP ports.  With
        /// increased use of Network Address Port Translation (NAPT) [14], this
        /// has become problematic, since maintaining multiple NAT bindings can
        /// be costly.  It also complicates firewall administration, since
        /// multiple ports must be opened to allow RTP traffic.  This memo
        /// discusses how the RTP and RTCP flows for a single media type can be
        /// run on a single port, to ease NAT traversal and simplify firewall
        /// administration, and considers when such multiplexing is appropriate.
        /// The multiplexing of several types of media (e.g., audio and video)
        /// onto a single port is not considered here (but see Section 5.2 of
        /// [1]).
        /// 
        /// https://tools.ietf.org/html/rfc5761
        /// </summary>
        public bool SupportsRtcpMultiplexing { get; set; } = true;

        /// <summary>
        /// &lt;port&gt; is the transport port to which the media stream is sent.  The
        /// meaning of the transport port depends on the network being used as
        /// specified in the relevant "c=" field, and on the transport
        /// protocol defined in the &lt;proto&gt; sub-field of the media field.
        /// Other ports used by the media application (such as the RTP Control
        /// Protocol (RTCP) port [19]) MAY be derived algorithmically from the
        /// base media port or MAY be specified in a separate attribute (for
        /// example, "a=rtcp:" as defined in [22]).
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// &lt;proto&gt; is the transport protocol.  The meaning of the transport
        /// protocol is dependent on the address type field in the relevant
        /// "c=" field.  Thus a "c=" field of IP4 indicates that the transport
        /// protocol runs over IP4.
        /// </summary>
        public string Protocol { get; set; }
        /// <summary>
        /// &lt;fmt&gt; is a media format description.  The fourth and any subsequent
        /// sub-fields describe the format of the media.  The interpretation
        /// of the media format depends on the value of the &lt;proto&gt; sub-field.
        /// 
        /// If the &lt;proto&gt; sub-field is "RTP/AVP" or "RTP/SAVP" the &lt;fmt&gt;
        /// sub-fields contain RTP payload type numbers.  When a list of
        /// payload type numbers is given, this implies that all of these
        /// payload formats MAY be used in the session, but the first of these
        /// formats SHOULD be used as the default format for the session.  For
        /// dynamic payload type assignments the "a=rtpmap:" attribute (see
        /// Section 6) SHOULD be used to map from an RTP payload type number
        /// to a media encoding name that identifies the payload format.  The
        /// "a=fmtp:"  attribute MAY be used to specify format parameters (see
        /// Section 6).
        /// </summary>
        public string Format { get; set; }

        public SendReceiveMode SendReceiveMode { get; set; } 

        public List<SdpIceCandidate> IceCandidates { get; } = new List<SdpIceCandidate>();
        public Dictionary<int, SdpMediaFormatDescription> MediaFormatDescriptions { get; internal set; } = new Dictionary<int, SdpMediaFormatDescription>();
        public SdpConnection Connection { get; set; }
        public SdpMediaSourceAttributes MediaSourceAttributes { get; set; } = new SdpMediaSourceAttributes();
        public SdpDtlsAttributes DtlsAttributes { get; set; } = new SdpDtlsAttributes();
        public SdpIceAttributes IceAttributes { get; set; } = new SdpIceAttributes();

        /// <summary>
        /// The "a=control:" attribute is used to convey the control URL. This
        /// attribute is used both for the session and media descriptions. If
        /// used for individual media, it indicates the URL to be used for
        /// controlling that particular media stream. If found at the session
        /// level, the attribute indicates the URL for aggregate control.
        /// 
        /// Example:
        /// a=control:rtsp://example.com/foo
        /// 
        /// This attribute may contain either relative and absolute URLs,
        /// following the rules and conventions set out in RFC 1808 [25].
        /// Implementations should look for a base URL in the following order:
        /// 
        /// 1.     The RTSP Content-Base field
        /// 2.     The RTSP Content-Location field
        /// 3.     The RTSP request URL
        /// 
        /// If this attribute contains only an asterisk (*), then the URL is
        /// treated as if it were an empty embedded URL, and thus inherits the
        /// entire base URL.
        /// 
        /// https://tools.ietf.org/html/rfc2326#appendix-C.1.1
        /// </summary>
        public Uri RtspControlUrl { get; set; }

        /// <summary>
        /// https://tools.ietf.org/html/rfc4566#section-6
        /// 
        /// This gives the maximum video frame rate in frames/sec.  It is
        /// intended as a recommendation for the encoding of video data.
        /// Decimal representations of fractional values using the notation
        /// "&lt;integer&gt;.&lt;fraction&gt;" are allowed.  It is a media-level
        /// attribute, defined only for video media, and it is not
        /// dependent on charset.
        /// </summary>
        public decimal Framerate { get; set; }
    }
}
