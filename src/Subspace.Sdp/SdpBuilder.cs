using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Subspace.Sdp
{
    /// <summary>
    /// https://tools.ietf.org/html/rfc4566
    /// </summary>
    public class SdpBuilder
    {
        private readonly SdpSessionDescription _sessionDescription = new SdpSessionDescription();
        private readonly List<SdpMediaDescriptionBuilder> _mediaLineBuilders = new List<SdpMediaDescriptionBuilder>();

        public SdpSessionDescription SessionDescription => _sessionDescription;

        /// <summary>
        /// The "o=" field gives the originator of the session (her username and
        /// the address of the user's host) plus a session identifier and version
        /// number:
        /// </summary>
        /// <param name="sessionVersion">
        /// &lt;sess-version&gt; is a version number for this session description.  Its
        /// usage is up to the creating tool, so long as &lt;sess-version&gt; is
        /// increased when a modification is made to the session data.  Again,
        /// it is RECOMMENDED that an NTP format timestamp is used.
        /// </param>
        /// <param name="unicastAddr">
        /// &lt;unicast-address&gt; is the address of the machine from which the
        /// session was created.  For an address type of IP4, this is either
        /// the fully qualified domain name of the machine or the dotted-
        /// decimal representation of the IP version 4 address of the machine.
        /// For an address type of IP6, this is either the fully qualified
        /// domain name of the machine or the compressed textual
        /// representation of the IP version 6 address of the machine.  For
        /// both IP4 and IP6, the fully qualified domain name is the form that
        /// SHOULD be given unless this is unavailable, in which case the
        /// globally unique address MAY be substituted.  A local IP address
        /// MUST NOT be used in any context where the SDP description might
        /// leave the scope in which the address is meaningful (for example, a
        /// local address MUST NOT be included in an application-level
        /// referral that might leave the scope).
        /// </param>
        public SdpBuilder SetOrigin(int sessionVersion, string unicastAddr)
        {
            var randomSid = new byte[8];
            RandomNumberGenerator.Fill(randomSid);
            randomSid[7] = 0;
            _sessionDescription.Origin = new SdpOrigin
            {
                SessionId = BitConverter.ToUInt64(randomSid),
                SessionVersion = sessionVersion,
                AddrType = "IP4",
                UnicastAddr = unicastAddr
            };
            return this;
        }

        /// <summary>
        /// s=&lt;session name&gt;
        /// 
        /// The "s=" field is the textual session name.  There MUST be one and
        /// only one "s=" field per session description.  The "s=" field MUST NOT
        /// be empty and SHOULD contain ISO 10646 characters (but see also the
        /// "a=charset" attribute).  If a session has no meaningful name, the
        /// value "s= " SHOULD be used (i.e., a single space as the session
        /// name).
        /// </summary>
        public SdpBuilder SetSessionName(string sessionName)
        {
            _sessionDescription.SessionName = sessionName;
            return this;
        }

        /// <summary>
        /// t=&lt;start-time&gt; &lt;stop-time&gt;
        /// 
        /// The "t=" lines specify the start and stop times for a session.
        /// Multiple "t=" lines MAY be used if a session is active at multiple
        /// irregularly spaced times; each additional "t=" line specifies an
        /// additional period of time for which the session will be active.  If
        /// the session is active at regular times, an "r=" line (see below)
        /// should be used in addition to, and following, a "t=" line -- in which
        /// case the "t=" line specifies the start and stop times of the repeat
        /// sequence.
        /// </summary>
        public SdpBuilder SetTiming(long startTime, long endTime)
        {
            _sessionDescription.StartTime = startTime;
            _sessionDescription.EndTime = endTime;
            return this;
        }

        /// <summary>
        ///    This specification defines a new Session Description Protocol (SDP)
        /// Grouping Framework extension, 'BUNDLE'.  The extension can be used
        /// with the SDP Offer/Answer mechanism to negotiate the usage of a
        /// single transport (5-tuple) for sending and receiving media described
        /// by multiple SDP media descriptions ("m=" sections).  Such transport
        /// is referred to as a BUNDLE transport, and the media is referred to as
        /// bundled media.  The "m=" sections that use the BUNDLE transport form
        /// a BUNDLE group.
        /// 
        /// https://tools.ietf.org/html/draft-ietf-mmusic-sdp-bundle-negotiation-54
        /// https://tools.ietf.org/html/rfc5888#section-5
        /// </summary>
        public SdpBuilder SetBundle(params string[] mediaIds)
        {
            _sessionDescription.Bundles = new List<string>(mediaIds);
            return this;
        }

        /// <summary>
        /// https://tools.ietf.org/html/draft-ietf-mmusic-msid-13
        /// </summary>
        public SdpBuilder SetWebRtcMediaStreamId(string mediaStreamId)
        {
            _sessionDescription.WebRtcMediaStreamId = mediaStreamId;
            return this;
        }

        /// <summary>
        /// https://tools.ietf.org/html/rfc4566#section-5.14
        ///
        /// m=&lt;media&gt; &lt;port&gt; &lt;proto&gt; &lt;fmt&gt; ...
        /// 
        /// A session description may contain a number of media descriptions.
        /// Each media description starts with an "m=" field and is terminated by
        /// either the next "m=" field or by the end of the session description.
        /// A media field has several sub-fields:
        /// </summary>
        /// <param name="mediaId">
        /// Used for identifying media streams within a
        /// session description.
        /// 
        /// https://tools.ietf.org/html/rfc5888#section-4
        /// </param>
        /// <param name="media">
        /// &lt;media&gt; is the media type.  Currently defined media are "audio",
        /// "video", "text", "application", and "message", although this list
        /// may be extended in the future (see Section 8).
        /// </param>
        /// <param name="port">
        /// &lt;port&gt; is the transport port to which the media stream is sent.  The
        /// meaning of the transport port depends on the network being used as
        /// specified in the relevant "c=" field, and on the transport
        /// protocol defined in the &lt;proto&gt; sub-field of the media field.
        /// Other ports used by the media application (such as the RTP Control
        /// Protocol (RTCP) port [19]) MAY be derived algorithmically from the
        /// base media port or MAY be specified in a separate attribute (for
        /// example, "a=rtcp:" as defined in [22]).
        /// </param>
        /// <param name="protocol">
        /// &lt;proto&gt; is the transport protocol.  The meaning of the transport
        /// protocol is dependent on the address type field in the relevant
        /// "c=" field.  Thus a "c=" field of IP4 indicates that the transport
        /// protocol runs over IP4.
        /// </param>
        public SdpMediaDescriptionBuilder AddMediaDescription(string mediaId, string media, int port, string protocol = "UDP/TLS/RTP/SAVPF")
        {
            var sdpMediaDescription = new SdpMediaDescription
            {
                MediaId = mediaId,
                Media = media,
                Port = port,
                Protocol = protocol
            };

            _sessionDescription.MediaDescriptions.Add(sdpMediaDescription);
            var mediaDescriptionBuilder = new SdpMediaDescriptionBuilder(sdpMediaDescription, this);

            _mediaLineBuilders.Add(mediaDescriptionBuilder);

            return mediaDescriptionBuilder;
        }

        public string Build()
        {
            var builder = new StringBuilder();
            builder.AddSdpLine("v", "0");
            builder.AddSdpLine("o", $"{_sessionDescription.Origin.Username} {_sessionDescription.Origin.SessionId} {_sessionDescription.Origin.SessionVersion} {_sessionDescription.Origin.NetType} {_sessionDescription.Origin.AddrType} {_sessionDescription.Origin.UnicastAddr}");
            builder.AddSdpLine("s", $"{_sessionDescription.SessionName}");
            builder.AddSdpLine("t", $"{_sessionDescription.StartTime} {_sessionDescription.EndTime}");
            builder.AddSdpLine("a", $"group:BUNDLE {string.Join(" ", _sessionDescription.Bundles)}");
            builder.AddSdpLine("a", $"msid-semantic: WMS {_sessionDescription.WebRtcMediaStreamId}");

            foreach (var mediaLineBuilder in _mediaLineBuilders)
            {
                builder.Append(mediaLineBuilder.Build());
            }

            return builder.ToString();
        }
    }

    public static class SdpBuilderExtensions
    {
        public static void AddSdpLine(this StringBuilder builder, string key, string value)
        {
            builder.Append(key + "=" + value + "\r\n");
        }
    }
}
