using System;
using System.Net;
using Subspace.Dtls;
using Subspace.Rtp.Srtp;

namespace Subspace.WebRtc
{
    public class PeerConnection
    {
        public SrtpPolicy SrtpPolicy { get; set; }
        public IPEndPoint IpRemoteEndpoint { get; set; }
        public DtlsContext DtlsContext { get; set; }
        public DateTime LastReceiveTimestamp { get; set; }
    }
}
