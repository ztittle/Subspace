using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Subspace.Ice;
using Subspace.Sdp;
using Subspace.Stun;
using Subspace.WebRtc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RtspProxy
{
    public interface IWebSocketSignalingServer
    {
        Task ProcessWebSocketAsync(HttpContext httpContext, WebSocket webSocket);
    }

    public class WebSocketSignalingServer : IWebSocketSignalingServer
    {
        private readonly IWebRtcServer _webRtcServer;
        private readonly ILogger<WebSocketSignalingServer> _logger;
        private readonly IStunHandler _stunHandler;
        private readonly IRtspProxyService _rtspProxyService;

        public WebSocketSignalingServer(
            ILogger<WebSocketSignalingServer> logger,
            IWebRtcServer webRtcServer,
            IStunHandler stunHandler,
            IRtspProxyService rtspProxyService)
        {
            _logger = logger;
            _webRtcServer = webRtcServer;
            _stunHandler = stunHandler;
            _rtspProxyService = rtspProxyService;
        }

        public async Task ProcessWebSocketAsync(HttpContext httpContext, WebSocket webSocket)
        {
            var clientIp = new IPEndPoint(httpContext.Connection.RemoteIpAddress, httpContext.Connection.RemotePort);
            var host = httpContext.Request.Host.Host;

            _logger.LogDebug("[{clientIp}] Client connected", clientIp);

            var buf = new byte[8092];
            var result = await webSocket.ReceiveAsync(buf, CancellationToken.None);
            string iceUserLocal = null;

            while (!result.CloseStatus.HasValue)
            {
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var requestPayload = Encoding.UTF8.GetString(buf, 0, result.Count);
                    var clientReq = JsonSerializer.Deserialize<WSPayload>(requestPayload);

                    _logger.LogDebug("[{clientIp}] WS Request '{action}' received", clientIp, clientReq.action);
                    _logger.LogTrace("[{clientIp}] {requestPayload}", clientIp, requestPayload);

                    if (clientReq.action == WSAction.PlayStream)
                    {
                        await ProcessPlayStreamAsync(webSocket, clientIp, clientReq);
                    }

                    if (clientReq.action == WSAction.CreateSdpOffer && _rtspProxyService.CurrentStream != null)
                    {
                        IceUtils.GenerateIceUsernamePassword(out iceUserLocal, out var icePasswordLocal);

                        await ProcessCreateSdpOfferAsync(webSocket, clientIp, host, iceUserLocal, icePasswordLocal);
                    }

                    if (clientReq.action == WSAction.SelectIceCandidate)
                    {
                        await ProcessSelectIceCandidateAsync(iceUserLocal, clientReq);
                    }
                }

                try
                {
                    result = await webSocket.ReceiveAsync(buf, CancellationToken.None);
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e, "[{clientIp}] receive error.", clientIp);
                    return;
                }
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private async Task ProcessPlayStreamAsync(WebSocket webSocket, IPEndPoint clientIp, WSPayload clientReq)
        {
            try
            {
                await _rtspProxyService.PlayStreamAsync(clientReq.rtspUrl);
            }
            catch (Exception e)
            {
                await SendResponseAsync(webSocket, clientIp,
                    new WSPayload
                    {
                        action = clientReq.action,
                        errorMessage = e.Message,
                        error = true
                    });
            }
        }

        private async Task ProcessCreateSdpOfferAsync(WebSocket webSocket, IPEndPoint clientIp, string host, string iceUserLocal, string iceLocalPassword)
        {
            var serverAddr = (await Dns.GetHostAddressesAsync(host)).First(l => l.AddressFamily == AddressFamily.InterNetwork);
            var sdp = _rtspProxyService.BuildSdp(serverAddr, iceUserLocal, iceLocalPassword);

            var response = new WSPayload
            {
                action = WSAction.CreateSdpOffer,
                type = "offer",
                sdp = sdp
            };

            await SendResponseAsync(webSocket, clientIp, response);
        }

        private async Task SendResponseAsync(WebSocket webSocket, IPEndPoint clientIp, WSPayload response)
        {
            var responseData = JsonSerializer.Serialize(response);
            _logger.LogDebug("[{clientIp}] Sending response", clientIp);
            _logger.LogTrace("[{clientIp}] {responseData}", clientIp, responseData);

            await webSocket.SendAsync(Encoding.UTF8.GetBytes(responseData), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
        }

        private async Task ProcessSelectIceCandidateAsync(string iceUserLocal, WSPayload clientReq)
        {
            var sdpParser = new SdpParser();
            var clientSdp = sdpParser.Parse(clientReq.sdp);

            var iceCandidate = clientSdp.MediaDescriptions
                .SelectMany(l => l.IceCandidates)
                .FirstOrDefault(l => l.CandidateType == "host");

            var iceAttributes = clientSdp.MediaDescriptions
                .Select(l => l.IceAttributes)
                .FirstOrDefault();

            if (iceCandidate != null && iceAttributes != null)
            {
                try
                {
                    var iceClientIps = await Dns.GetHostEntryAsync(iceCandidate.ConnectionAddress);
                    var iceClientIp = iceClientIps.AddressList.First();
                    var clientIceEndpoint = new IPEndPoint(iceClientIp, iceCandidate.Port);

                    await SendIceCandidateAsync(clientIceEndpoint, iceUserLocal, iceCandidate, iceAttributes);
                }
                catch (SocketException e) when (e.SocketErrorCode == SocketError.HostNotFound)
                {
                    _logger.LogInformation("Unable to resolve ice host {ConnectionAddress} to an IP Address", iceCandidate.ConnectionAddress);
                }
            }
        }

        private async Task SendIceCandidateAsync(IPEndPoint clientIceEndpoint, string iceUserLocal, SdpIceCandidate iceCandidate, SdpIceAttributes iceAttributes)
        {
            var responseRecord = new StunRecord
            {
                MessageType = StunMessageType.BindingRequest,
                MessageTransactionId = Guid.NewGuid().ToByteArray().AsSpan(0, 12).ToArray(),
                StunAttributes = new List<StunAttribute>
                {
                    new StunAttribute { Type = StunAttributeType.UseCandidate },
                    new PriorityAttribute { Priority = IceUtils.GenerateIcePriority(126, 65535, iceCandidate.ComponentId) },
                    new IceControllingAttribute { TieBreaker = IceUtils.GetRandomTieBreaker() },
                    new UsernameAttribute
                    {
                        Username = $"{iceAttributes.UsernameFragment}:{iceUserLocal}"
                    }
                }
            };

            responseRecord.Sign(iceAttributes.Password);

            await _stunHandler.SendResponseAsync(clientIceEndpoint, responseRecord);
        }

        public class WSPayload
        {
            public WSAction action { get; set; }
            public string type { get; set; }
            public string sdp { get; set; }
            public string rtspUrl { get; set; }
            public bool error { get; set; }
            public string errorMessage { get; set; }
        }

        public enum WSAction
        {
            CreateSdpOffer,
            SelectIceCandidate,
            PlayStream
        }
    }
}
