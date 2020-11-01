using System.Collections.Generic;

namespace Subspace.Rtsp
{
    public class RtspOptionsResponse
    {
        public IReadOnlyCollection<string> AllowedMethods { get; internal set; }
        public RtspResponseMessage ResponseMessage { get; internal set; }
    }
}
