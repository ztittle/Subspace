using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Subspace.Rtp.Rtcp
{
    public interface IRtcpClient
    {
        int Port { get; }
        Task<IReadOnlyList<RtcpPacket>> ReceiveAsync();
        Task SendAsync(IPEndPoint remoteEndpoint, params RtcpPacket[] rtcpPackets);
    }

    public class RtcpClient : IRtcpClient
    {
        private readonly UdpClient _udpClient;

        public RtcpClient()
        {
            _udpClient = new UdpClient(new IPEndPoint(0L, 0));

            Debug.WriteLine($"Listening on {_udpClient.Client.LocalEndPoint}", nameof(RtcpClient));
            _udpClient.Client.IgnoreConnectionReset();
        }

        public int Port => ((IPEndPoint)_udpClient.Client.LocalEndPoint).Port;

        public async Task<IReadOnlyList<RtcpPacket>> ReceiveAsync()
        {
            try
            {
                var result = await _udpClient.ReceiveAsync();

                var rtcpPackets = new List<RtcpPacket>();

                var read = 0;

                while (read < result.Buffer.Length)
                {
                    var rtcpPacket = RtcpPacketParser.ParseRtcpPacket(result.Buffer.AsSpan(read));
                    read += rtcpPacket.LengthIn32BitWordsMinusOne * 4 + 4;
                    rtcpPacket.RemoteEndPoint = result.RemoteEndPoint;

                    rtcpPackets.Add(rtcpPacket);
                }

                return rtcpPackets;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception when receiving bytes. {e}", nameof(RtcpClient));
                throw;
            }
        }

        public async Task SendAsync(IPEndPoint remoteEndpoint, params RtcpPacket[] rtcpPackets)
        {
            var bufferLength = rtcpPackets.Sum(l => l.LengthIn32BitWordsMinusOne * 4 + 4);
            var buffer = new byte[bufferLength];

            var idx = 0;
            foreach (var packet in rtcpPackets)
            {
                RtcpPacketSerializer.Serialize(packet, buffer.AsSpan(idx));

                idx += packet.LengthIn32BitWordsMinusOne * 4 + 4;
            }

            await _udpClient.SendAsync(buffer, bufferLength, remoteEndpoint);
        }
    }
}
