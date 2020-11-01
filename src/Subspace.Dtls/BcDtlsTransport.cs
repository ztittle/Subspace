using System.IO;
using System.Net;
using System.Net.Sockets;

namespace Subspace.Dtls
{
    internal class BcDtlsTransport : Org.BouncyCastle.Crypto.Tls.DatagramTransport
    {
        private Stream _stream;
        private readonly Socket _socket;
        private readonly IPEndPoint _remoteEndpoint;

        public BcDtlsTransport(Stream stream, Socket socket, IPEndPoint remoteEndpoint)
        {
            _stream = stream;
            _socket = socket;
            _remoteEndpoint = remoteEndpoint;
        }

        public void Close()
        {
        }

        public int GetReceiveLimit()
        {
            return 65535;
        }

        public int GetSendLimit()
        {
            return 65535;
        }

        public int Receive(byte[] buf, int off, int len, int waitMillis)
        {
            //return _socket.Receive(buf, off, len, SocketFlags.None);
            return _stream.Read(buf, off, len);
        }

        public void Send(byte[] buf, int off, int len)
        {
            _socket.SendTo(buf, off, len, SocketFlags.None, _remoteEndpoint);
        }

        internal void SetStream(Stream requestStream)
        {
            _stream = requestStream;
        }
    }
}
