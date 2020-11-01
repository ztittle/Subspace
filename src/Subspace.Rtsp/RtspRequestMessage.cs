using System;
using System.Collections.Generic;
using System.IO;

namespace Subspace.Rtsp
{
    public class RtspRequestMessage
    {
        public RtspRequestMessage(Uri rtspUri, string method)
        {
            RtspUri = rtspUri;
            Method = method;
        }

        public Uri RtspUri { get; set; }
        public string Method { get; set; }
        public List<KeyValuePair<string, string>> Headers { get; } = new List<KeyValuePair<string, string>>();
        public MemoryStream Content { get; } = new MemoryStream();
    }
}
