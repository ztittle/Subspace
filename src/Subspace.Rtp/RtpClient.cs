using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Subspace.Rtp
{
    public interface IRtpClient
    {
        int Port { get; }
        Task<RtpPacket> ReceiveAsync();
    }

    public class RtpPacketBuffer
    {
        public ushort? SeqNumber { get; set; }
        public List<RtpPacket> Buffer { get; } = new List<RtpPacket>();
    }

    public class RtpClient : IRtpClient
    {
        private static TimeSpan _receiveTimeout = TimeSpan.FromSeconds(5);

        private readonly UdpClient _udpClient;
        private readonly ConcurrentDictionary<uint, RtpPacketBuffer> _rtpPacketBuffer = new ConcurrentDictionary<uint, RtpPacketBuffer>();

        public RtpClient()
        {
            _udpClient = new UdpClient(new IPEndPoint(0L, 0));
            Debug.WriteLine($"Listening on {_udpClient.Client.LocalEndPoint}", nameof(RtpClient));
            _udpClient.Client.ReceiveBufferSize = int.MaxValue;
            _udpClient.Client.IgnoreConnectionReset();
        }

        public int Port => ((IPEndPoint)_udpClient.Client.LocalEndPoint).Port;

        public async Task<RtpPacket> ReceiveAsync()
        {
            var rtpPacket = await ReceiveUnorderedAsync();

            // todo: reorder packets

            return rtpPacket;
        }

        private async Task<RtpPacket> ReceiveUnorderedAsync()
        {
            try
            {
                var result = await ReceiveRawBytesAsync();

                var rtpPacket = RtpPacketParser.ParseRtpPacket(result.Buffer);
                rtpPacket.RemoteEndPoint = result.RemoteEndPoint;

                return rtpPacket;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception when receiving bytes. {e}", nameof(RtpClient));
                throw;
            }
        }

        private async Task<UdpReceiveResult> ReceiveRawBytesAsync()
        {
            UdpReceiveResult result;

            var timeoutTask = Task.Delay(_receiveTimeout);
            var receiveTask = _udpClient.ReceiveAsync();

            var completedTask = await Task.WhenAny(receiveTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                Debug.WriteLine("No RTP packets received in 5 seconds", nameof(RtpClient));
            }

            result = await receiveTask;

            return result;
        }
    }
}
