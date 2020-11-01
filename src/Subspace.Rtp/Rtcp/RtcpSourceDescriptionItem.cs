namespace Subspace.Rtp.Rtcp
{
    public class RtcpSourceDescriptionItem
    {
        public SourceDescriptionType Type { get; set; }

        /// <summary>
        /// The text is encoded according to the UTF-8 encoding specified in RFC
        /// 2279 [5].  US-ASCII is a subset of this encoding and requires no
        /// additional encoding.  The presence of multi-octet encodings is
        /// indicated by setting the most significant bit of a character to a
        /// value of one.
        /// </summary>
        public string Text { get; set; }
    }
}