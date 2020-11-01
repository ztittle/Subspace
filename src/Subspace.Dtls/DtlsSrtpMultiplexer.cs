using Subspace.Rtp;
using Subspace.Stun;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Subspace.Dtls
{
    public interface IDtlSrtpMultiplexer
    {
        Socket Socket { set; }
        Task ProcessRequestAsync(IPEndPoint remoteEndPoint, DtlsContext context, byte[] requestBytes);
    }

    public class DtlsSrtpMultiplexer : IDtlSrtpMultiplexer
    {
        private readonly IStunHandler _stunHandler;
        private readonly IDtlsHandler _dtlsHandler;
        private readonly IRtpHandler _srtpHandler;
        private Socket _socket;

        public DtlsSrtpMultiplexer(
            IStunHandler stunServer,
            IDtlsHandler dtlsServer,
            IRtpHandler srtpHandler)
        {
            _stunHandler = stunServer;
            _dtlsHandler = dtlsServer;
            _srtpHandler = srtpHandler;
        }

        public Socket Socket
        {
            set
            {
                _socket = value;
                _stunHandler.Socket = value;
                _dtlsHandler.Socket = value;
                _srtpHandler.Socket = value;
            }
        }

        public async Task ProcessRequestAsync(IPEndPoint remoteEndPoint, DtlsContext context, byte[] requestBytes)
        {
            if (requestBytes.Length == 0)
            {
                return;
            }

            var protocolType = DetermineProtocolType(requestBytes);

            switch (protocolType)
            {
                case ProtocolType.STUN:
                    await _stunHandler.ProcessRequestAsync(requestBytes, remoteEndPoint);
                    break;
                case ProtocolType.DTLS:
                    await _dtlsHandler.ProcessRequestAsync(requestBytes, remoteEndPoint, context);
                    break;
                case ProtocolType.RTP:
                    await _srtpHandler.ProcessRequestAsync(requestBytes, remoteEndPoint);
                    break;
            }
        }

        /// <summary>
        /// https://tools.ietf.org/html/rfc5764#section-5.1.2
        /// </summary>
        private ProtocolType DetermineProtocolType(byte[] requestBuffer)
        {
            if (requestBuffer.Length == 0)
            {
                return ProtocolType.Unknown;
            }

            var firstByte = requestBuffer[0];

            return firstByte switch
            {
                var b when b > 127 && b < 192 => ProtocolType.RTP,
                var b when b > 19 && b < 64 => ProtocolType.DTLS,
                var b when b < 2 => ProtocolType.STUN,
                _ => ProtocolType.Unknown,
            };
        }
    }
}
