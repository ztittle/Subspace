namespace Subspace.Sdp
{
    /// <summary>
    /// https://tools.ietf.org/html/rfc5245#section-15
    /// </summary>
    public class SdpIceAttributes
    {
        /// <summary>
        /// The "ice-pwd" and "ice-ufrag" attributes can appear at either the
        /// session-level or media-level.  When present in both, the value in the
        /// media-level takes precedence.  Thus, the value at the session-level
        /// is effectively a default that applies to all media streams, unless
        /// overridden by a media-level value.  Whether present at the session or
        /// media-level, there MUST be an ice-pwd and ice-ufrag attribute for
        /// each media stream.  If two media streams have identical ice-ufrag's,
        /// they MUST have identical ice-pwd's.
        /// 
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
        /// 
        /// https://tools.ietf.org/html/rfc5245#section-15.4
        /// </summary>
        /// 
        public string UsernameFragment { get; set; }
        /// <summary>
        /// The "ice-pwd" and "ice-ufrag" attributes can appear at either the
        /// session-level or media-level.  When present in both, the value in the
        /// media-level takes precedence.  Thus, the value at the session-level
        /// is effectively a default that applies to all media streams, unless
        /// overridden by a media-level value.  Whether present at the session or
        /// media-level, there MUST be an ice-pwd and ice-ufrag attribute for
        /// each media stream.  If two media streams have identical ice-ufrag's,
        /// they MUST have identical ice-pwd's.
        /// 
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
        /// 
        /// https://tools.ietf.org/html/rfc5245#section-15.4
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The "ice-options" attribute is a session-level attribute.  It
        /// contains a series of tokens that identify the options supported by
        /// the agent.  Its grammar is:
        /// 
        /// ice-options           = "ice-options" ":" ice-option-tag
        /// 0*(SP ice-option-tag)
        /// ice-option-tag        = 1*ice-char
        /// 
        /// https://tools.ietf.org/html/rfc5245#section-15.5
        /// </summary>
        public string Options { get; set; }
    }
}
