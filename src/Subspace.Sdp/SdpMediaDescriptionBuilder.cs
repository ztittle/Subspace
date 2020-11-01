using System;
using System.Linq;
using System.Text;

namespace Subspace.Sdp
{
    public class SdpMediaDescriptionBuilder
    {
        private readonly SdpMediaDescription _sdpMediaDescription;
        private readonly SdpBuilder _sdpBuilder;

        public SdpMediaDescription MediaDescription => _sdpMediaDescription;

        public SdpMediaDescriptionBuilder(SdpMediaDescription sdpMediaDescription, SdpBuilder sdpBuilder)
        {
            _sdpMediaDescription = sdpMediaDescription;
            _sdpBuilder = sdpBuilder;
        }

        public SdpBuilder SdpBuilder()
        {
            return _sdpBuilder;
        }

        /// <summary>
        /// https://tools.ietf.org/html/rfc4566#section-5.7
        ///
        /// c=&lt;nettype&gt; &lt;addrtype&gt; &lt;connection-address&gt;
        /// 
        /// The "c=" field contains connection data.
        /// 
        /// A session description MUST contain either at least one "c=" field in
        /// each media description or a single "c=" field at the session level.
        /// It MAY contain a single session-level "c=" field and additional "c="
        /// field(s) per media description, in which case the per-media values
        /// override the session-level settings for the respective media.
        /// </summary>
        /// <param name="address">
        /// The third sub-field ("&lt;connection-address&gt;") is the connection
        /// address.  OPTIONAL sub-fields MAY be added after the connection
        /// address depending on the value of the &lt;addrtype&gt; field.
        /// </param>
        public SdpMediaDescriptionBuilder SetConnection(string address)
        {
            _sdpMediaDescription.Connection = new SdpConnection
            {
                AddrType = "IP4",
                Address = address
            };

            return this;
        }

        public SdpMediaDescriptionBuilder SetSendReceiveMode(SendReceiveMode sendReceiveMode)
        {
            _sdpMediaDescription.SendReceiveMode = sendReceiveMode;
            return this;
        }

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
        public SdpMediaDescriptionBuilder SetSupportsRtcpMultiplexing(bool supports)
        {
            _sdpMediaDescription.SupportsRtcpMultiplexing = supports;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ssrc">
        /// The SDP media attribute "ssrc" indicates a property (known as a
        /// "source-level attribute") of a media source (RTP stream) within an
        /// RTP session. &lt;ssrc-id&gt; is the synchronization source (SSRC) ID of the
        /// source being described, interpreted as a 32-bit unsigned integer in
        /// network byte order and represented in decimal. &lt;attribute&gt; or
        /// &lt;attribute&gt;:&lt;value&gt; represents the source-level attribute specific to
        /// the given media source.  The source-level attribute follows the
        /// syntax of the SDP "a=" line.  It thus consists of either a single
        /// attribute name (a flag) or an attribute name and value, e.g.,
        /// "cname:user@example.com".  No attributes of the former type are
        /// defined by this document.
        /// </param>
        /// <param name="cname">
        /// The "cname" source attribute associates a media source with its
        /// Canonical End-Point Identifier (CNAME) source description (SDES)
        /// item.  This MUST be the CNAME value that the media sender will place
        /// in its RTCP SDES packets; it therefore MUST follow the syntax
        /// conventions of CNAME defined in the RTP specification [RFC3550].  If
        /// a session participant receives an RTCP SDES packet associating this
        /// SSRC with a different CNAME, it SHOULD assume there has been an SSRC
        /// collision and that the description of the source that was carried in
        /// the SDP description is not applicable to the actual source being
        /// received.  This source attribute is REQUIRED to be present if any
        /// source attributes are present for a source.  The "cname" attribute
        /// MUST NOT occur more than once for the same ssrc-id within a given
        /// media stream.
        /// </param>
        /// <param name="msid">
        /// The identifier is a string of ASCII characters that are legal in a
        /// "token", consisting of between 1 and 64 characters.
        /// </param>
        /// <param name="msIdAppData">
        /// Application data (msid-appdata) is carried on the same line as the
        /// identifier, separated from the identifier by a space.
        /// </param>
        public SdpMediaDescriptionBuilder SetMediaStream(uint ssrc, string cname = null, string msid = null, string msIdAppData = null)
        {
            _sdpMediaDescription.MediaSourceAttributes = new SdpMediaSourceAttributes
            {
                Ssrc = ssrc,
                CName = cname ?? Guid.NewGuid().ToString("N"),
                MsId = msid ?? Guid.NewGuid().ToString("N"),
                MsIdAppData = msIdAppData ?? Guid.NewGuid().ToString("N")
            };
            return this;
        }

        /// <summary>
        /// The candidate attribute is a media-level attribute only.  It contains
        /// a transport address for a candidate that can be used for connectivity
        /// checks.
        /// 
        /// https://tools.ietf.org/html/rfc5245#section-15.1
        /// </summary>
        /// <param name="connectionAddress">
        /// Taken from RFC 4566 [RFC4566].  It is the
        /// IP address of the candidate, allowing for IPv4 addresses, IPv6
        /// addresses, and fully qualified domain names (FQDNs).  When parsing
        /// this field, an agent can differentiate an IPv4 address and an IPv6
        /// address by presence of a colon in its value - the presence of a
        /// colon indicates IPv6.  An agent MUST ignore candidate lines that
        /// include candidates with IP address versions that are not supported
        /// or recognized.  An IP address SHOULD be used, but an FQDN MAY be
        /// used in place of an IP address.  In that case, when receiving an
        /// offer or answer containing an FQDN in an a=candidate attribute,
        /// the FQDN is looked up in the DNS first using an AAAA record
        /// (assuming the agent supports IPv6), and if no result is found or
        /// the agent only supports IPv4, using an A.  If the DNS query
        /// returns more than one IP address, one is chosen, and then used for
        /// the remainder of ICE processing.
        /// </param>
        /// <param name="port">
        /// is also taken from RFC 4566 [RFC4566].  It is the port of
        /// the candidate.
        /// </param>
        public SdpMediaDescriptionBuilder AddIceCandidate(string connectionAddress, int port)
        {
            _sdpMediaDescription.IceCandidates.Add(new SdpIceCandidate
            {
                Foundation = 1,
                ComponentId = 1,
                Priority = _sdpMediaDescription.MediaSourceAttributes.Ssrc,
                ConnectionAddress = connectionAddress,
                Port = (ushort)port,
                CandidateType = "host"
            });
            return this;
        }

        /// <summary>
        /// This attribute maps from an RTP payload type number (as used in
        /// an "m=" line) to an encoding name denoting the payload format
        /// to be used.  It also provides information on the clock rate and
        /// encoding parameters.  It is a media-level attribute that is not
        /// dependent on charset.
        /// 
        /// For audio streams, &lt;encoding parameters&gt; indicates the number
        /// of audio channels.  This parameter is OPTIONAL and may be
        /// omitted if the number of channels is one, provided that no
        /// additional parameters are needed.
        /// 
        /// For video streams, no encoding parameters are currently
        /// specified.
        ///
        /// https://tools.ietf.org/html/rfc4566#section-6
        /// </summary>
        public SdpMediaDescriptionBuilder AddMediaFormat(int payloadType, string encodingName, int clockRate, string encodingParameters = null, string formatParameters = null, string[] rtcpFeedbackCapabilities = null)
        {
            _sdpMediaDescription.MediaFormatDescriptions[payloadType] = new SdpMediaFormatDescription
            {
                PayloadType = payloadType,
                EncodingName = encodingName,
                ClockRate = clockRate,
                EncodingParameters = encodingParameters,
                FormatParameters = formatParameters,
                RtcpFeedbackCapability = rtcpFeedbackCapabilities?.ToList()
            };

            return this;
        }

        /// <summary>
        /// This attribute maps from an RTP payload type number (as used in
        /// an "m=" line) to an encoding name denoting the payload format
        /// to be used.  It also provides information on the clock rate and
        /// encoding parameters.  It is a media-level attribute that is not
        /// dependent on charset.
        /// 
        /// For video streams, no encoding parameters are currently
        /// specified.
        ///
        /// https://tools.ietf.org/html/rfc4566#section-6
        /// </summary>
        public SdpMediaDescriptionBuilder AddVideoMediaFormat(int payloadType, string encodingName, int clockRate, string formatParameters = null, string[] rtcpFeedbackCapabilities = null)
        {
            _sdpMediaDescription.MediaFormatDescriptions[payloadType] = new SdpMediaFormatDescription
            {
                PayloadType = payloadType,
                EncodingName = encodingName,
                ClockRate = clockRate,
                FormatParameters = formatParameters,
                RtcpFeedbackCapability = rtcpFeedbackCapabilities?.ToList()
            };

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="usernameFragment">
        /// The ice-ufrag and ice-pwd attributes MUST be chosen randomly at the
        /// beginning of a session.  The ice-ufrag attribute MUST contain at
        /// least 24 bits of randomness, and the ice-pwd attribute MUST contain
        /// at least 128 bits of randomness.  This means that the ice-ufrag
        /// attribute will be at least 4 characters long, and the ice-pwd at
        /// least 22 characters long, since the grammar for these attributes
        /// allows for 6 bits of randomness per character.  The attributes MAY be
        /// longer than 4 and 22 characters, respectively, of course, up to 256
        /// characters.  The upper limit allows for buffer sizing in
        /// implementations.  Its large upper limit allows for increased amounts
        /// of randomness to be added over time.
        /// </param>
        /// <param name="password">
        /// The ice-ufrag and ice-pwd attributes MUST be chosen randomly at the
        /// beginning of a session.  The ice-ufrag attribute MUST contain at
        /// least 24 bits of randomness, and the ice-pwd attribute MUST contain
        /// at least 128 bits of randomness.  This means that the ice-ufrag
        /// attribute will be at least 4 characters long, and the ice-pwd at
        /// least 22 characters long, since the grammar for these attributes
        /// allows for 6 bits of randomness per character.  The attributes MAY be
        /// longer than 4 and 22 characters, respectively, of course, up to 256
        /// characters.  The upper limit allows for buffer sizing in
        /// implementations.  Its large upper limit allows for increased amounts
        /// of randomness to be added over time.
        /// </param>
        /// <param name="options">
        /// The "ice-options" attribute is a session-level attribute.  It
        /// contains a series of tokens that identify the options supported by
        /// the agent.
        /// </param>
        /// <returns></returns>
        public SdpMediaDescriptionBuilder SetIceParameters(string usernameFragment, string password, string options = "trickle")
        {
            _sdpMediaDescription.IceAttributes = new SdpIceAttributes
            {
                UsernameFragment = usernameFragment,
                Password = password,
                Options = options
            };
            return this;
        }

        /// <summary>
        /// https://tools.ietf.org/html/rfc4572
        /// </summary>
        /// <param name="fingerprint">
        /// A fingerprint is represented in SDP as an attribute (an 'a' line).
        /// It consists of the name of the hash function used, followed by the
        /// hash value itself.  The hash value is represented as a sequence of
        /// uppercase hexadecimal bytes, separated by colons.  The number of
        /// bytes is defined by the hash function.
        /// 
        /// https://tools.ietf.org/html/rfc4572#section-5
        /// </param>
        /// <param name="setup">
        /// The 'setup' attribute indicates which of the end points should
        /// initiate the TCP connection establishment (i.e., send the initial TCP
        /// SYN).  The 'setup' attribute is charset-independent and can be a
        /// session-level or a media-level attribute.  The following is the ABNF
        /// of the 'setup' attribute:
        /// 
        /// setup-attr           =  "a=setup:" role
        /// role                 =  "active" / "passive" / "actpass"
        /// / "holdconn"
        /// 
        /// 'active': The endpoint will initiate an outgoing connection.
        /// 
        /// 'passive': The endpoint will accept an incoming connection.
        /// 
        /// 'actpass': The endpoint is willing to accept an incoming
        /// connection or to initiate an outgoing connection.
        /// 
        /// 'holdconn': The endpoint does not want the connection to be
        /// established for the time being.
        /// 
        /// https://tools.ietf.org/html/rfc4145#section-4
        /// </param>
        public SdpMediaDescriptionBuilder SetDtlsParameters(string hashFunc, string fingerprint, string setup = "actpass")
        {
            _sdpMediaDescription.DtlsAttributes = new SdpDtlsAttributes
            {
                HashFunc = hashFunc,
                Fingerprint = fingerprint,
                Setup = setup
            };

            return this;
        }

        public string Build()
        {
            var builder = new StringBuilder();

            builder.AddSdpLine("m", $"{_sdpMediaDescription.Media} {_sdpMediaDescription.Port} {_sdpMediaDescription.Protocol} {string.Join(" ", _sdpMediaDescription.MediaFormatDescriptions.Keys)}");
            builder.AddSdpLine("c", $"{_sdpMediaDescription.Connection.NetType} {_sdpMediaDescription.Connection.AddrType} {_sdpMediaDescription.Connection.Address}");
            builder.AddSdpLine("a", _sdpMediaDescription.SendReceiveMode.ToSdpValue());
            builder.AddSdpLine("a", $"mid:{_sdpMediaDescription.MediaId}");
            if (_sdpMediaDescription.SupportsRtcpMultiplexing) builder.AddSdpLine("a", "rtcp-mux");
            builder.AddSdpLine("a", $"ice-ufrag:{_sdpMediaDescription.IceAttributes.UsernameFragment}");
            builder.AddSdpLine("a", $"ice-pwd:{_sdpMediaDescription.IceAttributes.Password}");
            builder.AddSdpLine("a", $"ice-options:{_sdpMediaDescription.IceAttributes.Options}");
            builder.AddSdpLine("a", $"fingerprint:{_sdpMediaDescription.DtlsAttributes.HashFunc} {_sdpMediaDescription.DtlsAttributes.Fingerprint}");
            builder.AddSdpLine("a", $"setup:{_sdpMediaDescription.DtlsAttributes.Setup}");
            foreach (var mediaFormatDescription in _sdpMediaDescription.MediaFormatDescriptions.Values)
            {
                builder.AddSdpLine("a", $"rtpmap:{mediaFormatDescription.PayloadType} {mediaFormatDescription.EncodingName}/{mediaFormatDescription.ClockRate}{(mediaFormatDescription.EncodingParameters != null ? $"/{mediaFormatDescription.EncodingParameters}" : "")}");
                if (mediaFormatDescription.FormatParameters != null)
                {
                    builder.AddSdpLine("a", $"fmtp:{mediaFormatDescription.PayloadType} {mediaFormatDescription.FormatParameters}");
                }

                if (mediaFormatDescription.RtcpFeedbackCapability != null)
                {
                    foreach (var rtcpFb in mediaFormatDescription.RtcpFeedbackCapability)
                    {
                        builder.AddSdpLine("a", $"rtcp-fb:{mediaFormatDescription.PayloadType} {rtcpFb}");
                    }
                }
            }
            builder.AddSdpLine("a", $"ssrc:{_sdpMediaDescription.MediaSourceAttributes.Ssrc} cname:{_sdpMediaDescription.MediaSourceAttributes.CName}");
            builder.AddSdpLine("a", $"ssrc:{_sdpMediaDescription.MediaSourceAttributes.Ssrc} msid:{_sdpMediaDescription.MediaSourceAttributes.MsId} {_sdpMediaDescription.MediaSourceAttributes.MsIdAppData}");
            foreach (var iceCandidate in _sdpMediaDescription.IceCandidates)
            {
                builder.AddSdpLine("a", $"candidate:{iceCandidate.Foundation} {iceCandidate.ComponentId} {iceCandidate.Transport} {iceCandidate.Priority} {iceCandidate.ConnectionAddress} {iceCandidate.Port} typ {iceCandidate.CandidateType}");
            }
            builder.AddSdpLine("a", "end-of-candidates");

            return builder.ToString();
        }
    }
}
