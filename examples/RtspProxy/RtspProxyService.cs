using Microsoft.Extensions.Logging;
using Subspace.Dtls;
using Subspace.Rtsp;
using Subspace.Sdp;
using Subspace.Stun;
using Subspace.WebRtc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RtspProxy
{
    public interface IRtspProxyService
    {
        RtspStream CurrentStream { get; }
        Task PlayStreamAsync(string rtspUrl);
        string BuildSdp(IPAddress serverAddr, string iceUserLocal, string iceLocalPassword);
    }
    public class RtspProxyService : IRtspProxyService
    {
        private readonly IRtspPlayer _rtspPlayer;
        private readonly IWebRtcServer _webRtcServer;
        private readonly IWebRtcConnectionManager _webRtcConnectionManager;
        private readonly IDtlsHandler _dtlsServer;
        private readonly IStunUserProvider _stunUserProvider;
        private readonly ILogger<RtspProxyService> _logger;

        public RtspStream CurrentStream { get; private set; }

        public RtspProxyService(
            IRtspPlayer rtspPlayer,
            IWebRtcServer webRtcServer,
            IWebRtcConnectionManager webRtcConnectionManager,
            ILogger<RtspProxyService> logger,
            IDtlsHandler dtlsServer,
            IStunUserProvider stunUserProvider)
        {
            _rtspPlayer = rtspPlayer;
            _webRtcServer = webRtcServer;
            _webRtcConnectionManager = webRtcConnectionManager;
            _logger = logger;
            _dtlsServer = dtlsServer;
            _stunUserProvider = stunUserProvider;

            _rtspPlayer.RtpPacketReceived += RtpPacketReceived;
        }

        private async void RtpPacketReceived(object sender, Subspace.Rtp.RtpPacket rtpPacket)
        {
            var clients = _webRtcConnectionManager.GetAll();

            foreach (var client in clients) 
            {
                await _webRtcServer.SendRtpToClientAsync(client.IpRemoteEndpoint, rtpPacket);
            }
        }

        public async Task PlayStreamAsync(string rtspUrl)
        {
            if (CurrentStream != null)
            {
                await _rtspPlayer.CloseStreamAsync(CurrentStream);
            }

            var rtspUri = new Uri(rtspUrl);
            NetworkCredential credentials = null;
            if (rtspUri.UserInfo != null)
            {
                var userInfoParts = rtspUri.UserInfo.Split(':');
                credentials = new NetworkCredential(userInfoParts[0], userInfoParts[1]);
            }
            CurrentStream = await _rtspPlayer.PlayStreamAsync(rtspUri.OriginalString, credentials);
        }

        public string BuildSdp(IPAddress serverAddr, string iceUserLocal, string iceLocalPassword)
        {
            var serverAddrString = serverAddr.ToString();
            var sdpBuilder = new SdpBuilder();

            var serverPort = _webRtcServer.Endpoint.Port;

            const string name = "RtspProxy";

            _stunUserProvider.AddUser(iceUserLocal, iceLocalPassword);
            var dtlsFingerprint = _dtlsServer.GetSigningCredentialSha256Fingerprint();

            sdpBuilder
                .SetOrigin(1, serverAddrString)
                .SetSessionName(name)
                .SetTiming(0, 0)
                .SetWebRtcMediaStreamId(name);

            var mid = 1;
            foreach (var md in CurrentStream.Sdp.MediaDescriptions)
            {
                var mediaFormatDescriptions = md.MediaFormatDescriptions.Values;

                if (mediaFormatDescriptions.Count == 0)
                    continue;

                var mediaFormats = new List<SdpMediaFormatDescription>();
                foreach (var mfd in mediaFormatDescriptions)
                {
                    switch (md.Media)
                    {
                        case "audio" when mfd.EncodingName == "PCMA" && mfd.ClockRate <= 8000:
                        case "video" when mfd.EncodingName == "H264":
                            break;
                        default:
                            _logger.LogWarning("Unsupported {media} codec {encodingName}/{clockRate}", md.Media, mfd.EncodingName, mfd.ClockRate);
                            continue;
                    }

                    // safari hack
                    if (mfd.EncodingName == "H264")
                    {
                        mfd.FormatParameters = "profile-level-id=42e01f;packetization-mode=1";
                    }

                    mediaFormats.Add(mfd);
                }

                if (mediaFormats.Any())
                {
                    var mdBuilder = sdpBuilder
                        .AddMediaDescription(mid++.ToString(), md.Media, serverPort)
                        .SetConnection(serverAddrString)
                        .SetSendReceiveMode(SendReceiveMode.SendOnly)
                        .SetMediaStream(md.MediaSourceAttributes.Ssrc)
                        .AddIceCandidate(serverAddrString, serverPort)
                        .SetIceParameters(iceUserLocal, iceLocalPassword)
                        .SetDtlsParameters("sha-256", dtlsFingerprint);

                    foreach (var mfd in mediaFormats)
                    {
                        mdBuilder.AddMediaFormat(mfd.PayloadType, mfd.EncodingName, mfd.ClockRate, mfd.EncodingParameters, mfd.FormatParameters);
                    }
                }
            }

            sdpBuilder.SetBundle(sdpBuilder.SessionDescription.MediaDescriptions.Select(l => l.MediaId).ToArray());

            return sdpBuilder.Build();
        }
    }
}
