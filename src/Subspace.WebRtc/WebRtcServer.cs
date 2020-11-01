using Microsoft.Extensions.Options;
using Subspace.Dtls;
using Subspace.Rtp;
using Subspace.Rtp.Srtp;
using Subspace.Stun;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Subspace.WebRtc
{
    public interface IWebRtcServer
    {
        IDtlsHandler DtlsHandler { get; }
        IStunUserProvider StunUserProvider { get; }
        IPEndPoint Endpoint { get; }
        bool TranscodeAudioToOpus { get; set; }
        Task RunAsync();
        Task SendRtpToClientAsync(IPEndPoint clientEndpoint, RtpPacket rtp);
    }
    
    public class WebRtcServerOptions
    {
        public int RtcpMuxPort { get; set; }
    }

    public class WebRtcServer : IWebRtcServer
    {
        private IPEndPoint _endPoint;
        private UdpClient _udpClient;
        private WebRtcServerOptions _options;
        private readonly SrtpEncryptor _srtpEncryptor = new SrtpEncryptor();

        private readonly IDtlSrtpMultiplexer _webRtcDemultiplexer;
        private readonly IWebRtcConnectionManager _connectionManager;

        public IPEndPoint Endpoint => _endPoint;

        public bool TranscodeAudioToOpus { get; set; } = false;

        public IDtlsHandler DtlsHandler { get; }
        public IStunUserProvider StunUserProvider { get; }

        public const int SIO_UDP_CONNRESET = -1744830452;

        public WebRtcServer(
            IDtlSrtpMultiplexer webRtcDemultiplexer,
            IWebRtcConnectionManager connectionManager,
            IOptions<WebRtcServerOptions> options,
            IDtlsHandler dtlsHandler,
            IStunUserProvider stunUserProvider)
        {
            _webRtcDemultiplexer = webRtcDemultiplexer;
            _connectionManager = connectionManager;
            _options = options.Value;
            DtlsHandler = dtlsHandler;
            StunUserProvider = stunUserProvider;
        }

        public async Task RunAsync()
        {
            _udpClient = new UdpClient(new IPEndPoint(0L, _options.RtcpMuxPort));
            _udpClient.Client.IgnoreConnectionReset();

            _endPoint = (IPEndPoint)_udpClient.Client.LocalEndPoint;
            Debug.WriteLine($"Listening on {_endPoint}", nameof(WebRtcServer));

            _webRtcDemultiplexer.Socket = _udpClient.Client;



            while (true)
            {
                try
                {
                    var result = await _udpClient.ReceiveAsync();

                    var connection = _connectionManager.GetOrAdd(result.RemoteEndPoint);
                    connection.LastReceiveTimestamp = DateTime.UtcNow;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _webRtcDemultiplexer.ProcessRequestAsync(connection.IpRemoteEndpoint, connection.DtlsContext, result.Buffer);
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine(e.ToString(), nameof(WebRtcServer));
                        }
                    });
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.ToString(), nameof(WebRtcServer));
                }
            }
        }

        public async Task SendRtpToClientAsync(IPEndPoint clientEndpoint, RtpPacket rtp)
        {
            var webRtcConnection = _connectionManager.Get(clientEndpoint);

            if (webRtcConnection?.DtlsContext.DtlsKeyMaterial == null)
            {
                return;
            }

            var clientIpEndpoint = webRtcConnection.IpRemoteEndpoint;

            byte[] encryptedBytes;
            lock (webRtcConnection)
            {
                webRtcConnection.SrtpPolicy ??= DtlsSrtpUtils.CreateSrtpPolicy(webRtcConnection.DtlsContext.DtlsKeyMaterial);

                encryptedBytes = _srtpEncryptor.Encrypt(webRtcConnection.SrtpPolicy, rtp, clientIpEndpoint);
            }

            try
            {
                await _udpClient.SendAsync(encryptedBytes, encryptedBytes.Length, clientIpEndpoint);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"[{clientIpEndpoint}] Error sending SRTP to client {e}", nameof(WebRtcServer));
            }
        }
    }
}
