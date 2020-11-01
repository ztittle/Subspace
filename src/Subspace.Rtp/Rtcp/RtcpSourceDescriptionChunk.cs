using System.Collections.Generic;

namespace Subspace.Rtp.Rtcp
{
    public class RtcpSourceDescriptionChunk
    {

        /// <summary>
        /// SSRC: 32 bits
        /// 
        /// The synchronization source identifier
        /// </summary>
        public uint SynchronizationSource { get; set; }

        public List<RtcpSourceDescriptionItem> Items { get; set; }
    }
}