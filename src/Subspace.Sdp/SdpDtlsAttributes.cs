namespace Subspace.Sdp
{
    public class SdpDtlsAttributes
    {
        /// <summary>
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
        /// </summary>
        public string Setup { get; set; }

        /// <summary>
        /// A fingerprint is represented in SDP as an attribute (an 'a' line).
        /// It consists of the name of the hash function used, followed by the
        /// hash value itself.  The hash value is represented as a sequence of
        /// uppercase hexadecimal bytes, separated by colons.  The number of
        /// bytes is defined by the hash function.
        /// 
        /// https://tools.ietf.org/html/rfc4572#section-5
        /// </summary>
        public string HashFunc { get; set; }

        /// <summary>
        /// A fingerprint is represented in SDP as an attribute (an 'a' line).
        /// It consists of the name of the hash function used, followed by the
        /// hash value itself.  The hash value is represented as a sequence of
        /// uppercase hexadecimal bytes, separated by colons.  The number of
        /// bytes is defined by the hash function.
        /// 
        /// https://tools.ietf.org/html/rfc4572#section-5
        /// </summary>
        public string Fingerprint { get; set; }
    }
}
