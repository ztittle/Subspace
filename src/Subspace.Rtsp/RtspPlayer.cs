using Subspace.Rtp;
using Subspace.Rtp.Rtcp;
using Subspace.Sdp;
using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;

namespace Subspace.Rtsp
{
    public interface IRtspPlayer
    {
        event EventHandler<RtpPacket> RtpPacketReceived;
        event EventHandler<RtcpPacket> RtcpPacketReceived;
        Task<RtspStream> PlayStreamAsync(string uri, NetworkCredential credentials = null);
        Task CloseStreamAsync(RtspStream rtspStream);
    }

    public class RtspPlayer : IRtspPlayer
    {
        private readonly IRtspClient _rtspClient;
        private readonly IRtpClient _rtpClient;
        private readonly IRtcpClient _rtcpClient;
        private readonly IRtcpReceptionReportScheduler _receptionReportScheduler;

        public event EventHandler<RtpPacket> RtpPacketReceived;
        public event EventHandler<RtcpPacket> RtcpPacketReceived;

        public RtspPlayer(
            IRtspClient rtspClient,
            IRtpClient rtpClient,
            IRtcpClient rtcpClient,
            IRtcpReceptionReportScheduler receptionReportScheduler)
        {
            _rtspClient = rtspClient;
            _rtpClient = rtpClient;
            _rtcpClient = rtcpClient;
            _receptionReportScheduler = receptionReportScheduler;
            ReceiveRtpLoop();
            ReceiveRtcpLoop();
        }

        public async Task<RtspStream> PlayStreamAsync(string uri, NetworkCredential credentials = null)
        {
            var rtspStream = new RtspStream(uri, credentials);

            if (rtspStream.Credentials != null)
            {
                _rtspClient.AddCredential(rtspStream.Uri, rtspStream.Credentials);
            }

            var options = await _rtspClient.OptionsAsync(rtspStream.Uri);
            var describeResponse = await _rtspClient.DescribeAsync(rtspStream.Uri);

            var sdpParser = new SdpParser();
            rtspStream.Sdp = sdpParser.Parse(describeResponse.Sdp);

            var contentBaseHeaderValue =
                describeResponse.ResponseMessage.Headers
                    .Get("Content-Base") ??
                describeResponse.ResponseMessage.Headers
                    .Get("Content-Location");

            var contentBaseUrl = contentBaseHeaderValue != null ? new Uri(contentBaseHeaderValue) : rtspStream.Uri;

            string session = null;
            foreach (var md in rtspStream.Sdp.MediaDescriptions)
            {
                var rtspControlUrl = md.RtspControlUrl.IsAbsoluteUri
                    ? md.RtspControlUrl
                    : new Uri(contentBaseUrl, md.RtspControlUrl);

                var setupResponse = await _rtspClient.SetupAsync(rtspControlUrl, _rtpClient.Port, _rtcpClient.Port);
                md.MediaSourceAttributes.Ssrc = setupResponse.Ssrc;

                session ??= setupResponse.Session;
            }

            rtspStream.Session = session;

            var playResponse = await _rtspClient.PlayAsync(rtspStream.Uri, session, "0.000-");

            return rtspStream;
        }

        private void ReceiveRtpLoop()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var rtpPacket = await _rtpClient.ReceiveAsync();

                        _receptionReportScheduler.Track(rtpPacket);

                        RtpPacketReceived?.Invoke(this, rtpPacket);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Error Processing RTP Packet {e}", nameof(RtspClient));
                    }
                }
            });
        }

        private void ReceiveRtcpLoop()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        var rtcpPackets = await _rtcpClient.ReceiveAsync();

                        foreach (var pkt in rtcpPackets)
                        {
                            if (pkt is RtcpSenderReportPacket rtcpSenderReportPacket)
                            {
                                _receptionReportScheduler.SetSenderReport(rtcpSenderReportPacket);
                            }

                            RtcpPacketReceived?.Invoke(this, pkt);
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Error Processing RTCP Packet. {e}", nameof(RtspClient));
                    }
                }
            });
        }

        public async Task CloseStreamAsync(RtspStream rtspStream)
        {
            await _rtspClient.TeardownAsync(rtspStream.Uri, rtspStream.Session);
        }
    }
}
