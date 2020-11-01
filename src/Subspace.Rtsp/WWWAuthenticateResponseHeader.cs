namespace Subspace.Rtsp
{
    public enum ChallengeType
    {
        Basic,
        Digest
    }

    /// <summary>
    /// If a server receives a request for an access-protected object, and an
    /// acceptable Authorization header is not sent, the server responds with
    /// a "401 Unauthorized" status code, and a WWW-Authenticate header as
    /// per the framework defined above, which for the digest scheme is
    /// utilized as follows:
    /// 
    /// https://tools.ietf.org/html/rfc2617#section-3.2.1
    /// </summary>
    public class WWWAuthenticateResponseHeader
    {
        public ChallengeType Challenge { get; set; }
        /// <summary>
        /// A string to be displayed to users so they know which username and
        /// password to use. This string should contain at least the name of
        /// the host performing the authentication and might additionally
        /// indicate the collection of users who might have access. An example
        /// might be "registered_users@gotham.news.com".
        /// </summary>
        public string Realm { get; set; }

        /// <summary>
        /// A server-specified data string which should be uniquely generated
        /// each time a 401 response is made. It is recommended that this
        /// string be base64 or hexadecimal data. Specifically, since the
        /// string is passed in the header lines as a quoted string, the
        /// double-quote character is not allowed.
        /// 
        /// The contents of the nonce are implementation dependent. The quality
        /// of the implementation depends on a good choice. A nonce might, for
        /// example, be constructed as the base 64 encoding of
        /// 
        /// time-stamp H(time-stamp ":" ETag ":" private-key)
        /// 
        /// where time-stamp is a server-generated time or other non-repeating
        /// value, ETag is the value of the HTTP ETag header associated with
        /// the requested entity, and private-key is data known only to the
        /// server.  With a nonce of this form a server would recalculate the
        /// hash portion after receiving the client authentication header and
        /// reject the request if it did not match the nonce from that header
        /// or if the time-stamp value is not recent enough. In this way the
        /// server can limit the time of the nonce's validity. The inclusion of
        /// the ETag prevents a replay request for an updated version of the
        /// resource.  (Note: including the IP address of the client in the
        /// nonce would appear to offer the server the ability to limit the
        /// reuse of the nonce to the same client that originally got it.
        /// However, that would break proxy farms, where requests from a single
        /// user often go through different proxies in the farm. Also, IP
        /// address spoofing is not that hard.)
        /// 
        /// An implementation might choose not to accept a previously used
        /// nonce or a previously used digest, in order to protect against a
        /// replay attack. Or, an implementation might choose to use one-time
        /// nonces or digests for POST or PUT requests and a time-stamp for GET
        /// requests.  For more details on the issues involved see section 4.
        /// of this document.
        /// 
        /// The nonce is opaque to the client.
        /// </summary>
        public string Nonce { get; set; }
    }
}
