using Subspace.Rtp.Rtcp;
using Subspace.Rtp.Srtp;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Subspace.Rtp
{
    public interface IRtpHandler
    {
        Socket Socket { set; }
        Task ProcessRequestAsync(byte[] requestBytes, IPEndPoint remoteEndPoint);
    }

    public class RtpHandler : IRtpHandler
    {
        public Socket Socket { set; private get; }

        public Task ProcessRequestAsync(byte[] requestBytes, IPEndPoint remoteEndPoint)
        {
            if (IsRtcpPacket(requestBytes))
            {
                var rtcpPacket = RtcpPacketParser.ParseRtcpPacket(requestBytes);
            }
            else
            {
                var rtp = RtpPacketParser.ParseRtpPacket(requestBytes);
                // todo: srtp decryption

            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// New RTCP packet types may be registered in the future and will
        /// further reduce the RTP payload types that are available when
        /// multiplexing RTP and RTCP onto a single port.To allow this
        /// multiplexing, future RTCP packet type assignments SHOULD be made
        /// after the current assignments in the range 209-223, then in the range
        /// 194-199, so that only the RTP payload types in the range 64-95 are
        /// blocked.  RTCP packet types in the ranges 1-191 and 224-254 SHOULD
        /// only be used when other values have been exhausted.
        /// 
        /// https://tools.ietf.org/html/rfc5761#section-4
        /// </summary>
        private bool IsRtcpPacket(byte[] packetBytes)
        {
            var payloadTypeByte = packetBytes[1];

            return payloadTypeByte >= 200 && payloadTypeByte <= 204 ||
                   payloadTypeByte >= 209 && payloadTypeByte <= 223 ||
                   payloadTypeByte >= 194 && payloadTypeByte <= 199;
        }
    }
}
