namespace Subspace.Sdp
{
    /// <summary>
    ///    https://tools.ietf.org/html/rfc4566#section-5.2
    ///
    ///    The "o=" field gives the originator of the session (her username and
    ///    the address of the user's host) plus a session identifier and version
    ///    number:
    /// </summary>
    public class SdpOrigin
    {
        /// <summary>
        /// &lt;username&gt; is the user's login on the originating host, or it is "-"
        /// if the originating host does not support the concept of user IDs.
        /// The &lt;username&gt; MUST NOT contain spaces.
        /// </summary>
        public string Username { get; set; } = "-";

        /// <summary>
        /// &lt;sess-id&gt; is a numeric string such that the tuple of &lt;username&gt;,
        /// &lt;sess-id&gt;, &lt;nettype&gt;, &lt;addrtype&gt;, and &lt;unicast-address&gt; forms a
        /// globally unique identifier for the session.  The method of
        /// &lt;sess-id&gt; allocation is up to the creating tool, but it has been
        /// suggested that a Network Time Protocol (NTP) format timestamp be
        /// used to ensure uniqueness.
        /// </summary>
        public ulong SessionId { get; set; }

        /// <summary>
        /// &lt;sess-version&gt; is a version number for this session description.  Its
        /// usage is up to the creating tool, so long as &lt;sess-version&gt; is
        /// increased when a modification is made to the session data.  Again,
        /// it is RECOMMENDED that an NTP format timestamp is used.
        /// </summary>
        public long SessionVersion { get; set; }

        /// <summary>
        /// &lt;nettype&gt; is a text string giving the type of network.  Initially
        /// "IN" is defined to have the meaning "Internet", but other values
        /// MAY be registered in the future (see Section 8).
        /// </summary>
        public string NetType { get; set; } = "IN";

        /// <summary>
        /// &lt;addrtype&gt; is a text string giving the type of the address that
        /// follows.  Initially "IP4" and "IP6" are defined, but other values
        /// MAY be registered in the future (see Section 8).
        /// </summary>
        public string AddrType { get; set; }

        /// <summary>
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
        /// </summary>
        public string UnicastAddr { get; set; }
    }
}
