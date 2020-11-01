namespace Subspace.Sdp
{
    /// <summary>
    /// The candidate attribute is a media-level attribute only.  It contains
    /// a transport address for a candidate that can be used for connectivity
    /// checks.
    /// 
    /// https://tools.ietf.org/html/rfc5245#section-15.1
    /// </summary>
    public class SdpIceCandidate
    {
        /// <summary>
        /// is composed of 1 to 32 &lt;ice-char&gt;s.  It is an
        /// identifier that is equivalent for two candidates that are of the
        /// same type, share the same base, and come from the same STUN
        /// server.  The foundation is used to optimize ICE performance in the
        /// Frozen algorithm.
        /// </summary>
        public long Foundation { get; set; }

        /// <summary>
        /// positive integer between 1 and 256 that
        /// identifies the specific component of the media stream for which
        /// this is a candidate.  It MUST start at 1 and MUST increment by 1
        /// for each component of a particular candidate.  For media streams
        /// based on RTP, candidates for the actual RTP media MUST have a
        /// component ID of 1, and candidates for RTCP MUST have a component
        /// ID of 2.  Other types of media streams that require multiple
        /// components MUST develop specifications that define the mapping of
        /// components to component IDs.  See Section 14 for additional
        /// discussion on extending ICE to new media streams.
        /// </summary>
        public byte ComponentId { get; set; }

        /// <summary>
        /// indicates the transport protocol for the candidate.
        /// This specification only defines UDP.  However, extensibility is
        /// provided to allow for future transport protocols to be used with
        /// ICE, such as TCP or the Datagram Congestion Control Protocol
        /// (DCCP) [RFC4340].
        /// </summary>
        public string Transport { get; set; } = "UDP";

        /// <summary>
        /// is a positive integer between 1 and (2**31 - 1).
        /// </summary>
        public uint Priority { get; set; }

        /// <summary>
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
        /// </summary>
        public string ConnectionAddress { get; set; }

        /// <summary>
        /// is also taken from RFC 4566 [RFC4566].  It is the port of
        /// the candidate.
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// Encodes the type of candidate.  This specification
        /// defines the values "host", "srflx", "prflx", and "relay" for host,
        /// server reflexive, peer reflexive, and relayed candidates,
        /// respectively.  The set of candidate types is extensible for the
        /// future.
        /// </summary>
        public string CandidateType { get; set; }

    }
}
