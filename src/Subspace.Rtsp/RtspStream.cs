using Subspace.Sdp;
using System;
using System.Net;

namespace Subspace.Rtsp
{
    public class RtspStream
    {
        public RtspStream(string uri, NetworkCredential credentials)
        {
            Uri = new Uri(uri);
            Credentials = credentials;
        }

        public Uri Uri { get; }
        public NetworkCredential Credentials { get; }
        public SdpSessionDescription Sdp { get; internal set; }
        public string Session { get; internal set; }
    }
}
