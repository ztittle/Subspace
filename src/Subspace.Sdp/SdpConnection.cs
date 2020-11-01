namespace Subspace.Sdp
{
    public class SdpConnection
    {
        /// <summary>
        /// The first sub-field ("&lt;nettype&gt;") is the network type, which is a
        /// text string giving the type of network.  Initially, "IN" is defined
        /// to have the meaning "Internet", but other values MAY be registered in
        /// the future (see Section 8)
        /// </summary>
        public string NetType { get; set; } = "IN";
        /// <summary>
        /// The second sub-field ("&lt;addrtype&gt;") is the address type.  This allows
        /// SDP to be used for sessions that are not IP based.  This memo only
        /// defines IP4 and IP6, but other values MAY be registered in the future
        /// (see Section 8).
        /// </summary>
        public string AddrType { get; set; }
        /// <summary>
        /// The third sub-field ("&lt;connection-address&gt;") is the connection
        /// address.  OPTIONAL sub-fields MAY be added after the connection
        /// address depending on the value of the &lt;addrtype&gt; field.
        /// </summary>
        public string Address { get; set; }
    }
}
