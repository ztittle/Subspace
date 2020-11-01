namespace Subspace.Sdp
{
    /// <summary>
    /// https://tools.ietf.org/html/rfc5576#section-4.1
    /// </summary>
    public class SdpMediaSourceAttributes
    {
        /// <summary>
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
        /// 
        /// Within a media stream, "ssrc" attributes with the same value of
        /// &lt;ssrc-id&gt; describe different attributes of the same media sources.
        /// Across media streams, &lt;ssrc-id&gt; values are not correlated (unless
        /// correlation is indicated by media-stream grouping or some other
        /// mechanism) and MAY be repeated.
        /// 
        /// Each "ssrc" media attribute specifies a single source-level attribute
        /// for the given &lt;ssrc-id&gt;.  For each source mentioned in SDP, the
        /// source-level attribute "cname", defined in Section 6.1, MUST be
        /// provided.  Any number of other source-level attributes for the source
        /// MAY also be provided.
        /// </summary>
        public uint Ssrc { get; set; }
        /// <summary>
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
        /// 
        /// https://tools.ietf.org/html/rfc5576#section-6.1
        /// </summary>
        public string CName { get; set; }
        /// <summary>
        /// https://tools.ietf.org/html/draft-ietf-mmusic-msid-13
        /// 
        /// The identifier is a string of ASCII characters that are legal in a
        /// "token", consisting of between 1 and 64 characters.
        /// </summary>
        public string MsId { get; set; }
        /// <summary>
        /// Application data (msid-appdata) is carried on the same line as the
        /// identifier, separated from the identifier by a space.
        /// </summary>
        public string MsIdAppData { get; set; }
    }
}
