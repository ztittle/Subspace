using System.Collections.Generic;

namespace Subspace.Sdp
{
    /// <summary>
    /// A well-defined format for conveying sufficient
    /// information to discover and participate in a multimedia session.
    /// 
    /// https://tools.ietf.org/html/rfc4566#section-2
    /// </summary>
    public class SdpSessionDescription
    {
        public byte Version { get; set; }
        public SdpOrigin Origin { get; set; }
        public string SessionName { get; set; } = " ";
        public long StartTime { get; set; }
        public long EndTime { get; set; }
        public string WebRtcMediaStreamId { get; set; }
        public List<string> Bundles { get; set; }
        public List<SdpMediaDescription> MediaDescriptions { get; } = new List<SdpMediaDescription>();
    }
}
