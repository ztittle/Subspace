using Subspace.Rtp.Srtp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Subspace.Dtls
{
    public interface IDtlsHandler
    {
        Socket Socket { set; }
        string GetSigningCredentialSha256Fingerprint();
        Task ProcessRequestAsync(byte[] requestBytes, IPEndPoint remoteEndPoint, DtlsContext context);
    }

    public class DtlsHandler : IDtlsHandler
    {
        private readonly Org.BouncyCastle.Crypto.Tls.DtlsServerProtocol _dtlsServerProtocol = new Org.BouncyCastle.Crypto.Tls.DtlsServerProtocol(new Org.BouncyCastle.Security.SecureRandom());
        private X509Certificate2 _signingCredentialCert;
        private Socket _socket;

        public DtlsHandler()
        {
            SetSigningCredentials();
        }

        public Socket Socket
        {
            set => _socket = value;
        }

        public void SetSigningCredentials()
        {
            var rsa = RSA.Create();
            var req = new CertificateRequest("cn=Subspace", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var now = DateTimeOffset.Now;
            _signingCredentialCert = req.CreateSelfSigned(now, now.AddYears(5));
        }

        public Task ProcessRequestAsync(byte[] requestBytes, IPEndPoint remoteEndPoint, DtlsContext context)
        {
            var requestStream = new MemoryStream(requestBytes);

            if (context.BcDtlsTransport is null)
            {
                Debug.WriteLine($"[{remoteEndPoint}] Opening DTLS Connection", nameof(DtlsHandler));
                context.BcDtlsTransport = new BcDtlsTransport(requestStream, _socket, remoteEndPoint);

                var bcDtlsServer = new BcDtlsServer(_signingCredentialCert);
                context.BcDtlsServer = bcDtlsServer;
                bcDtlsServer.HandshakeComplete = () =>
                {
                    context.DtlsKeyMaterial = bcDtlsServer.GetKeyMaterial(SrtpConstants.SrtpDefaultMasterKeyKeyLength * 2 +
                                                                          SrtpConstants.SrtpDefaultMasterKeySaltLength *
                                                                          2);
                };
                _dtlsServerProtocol.Accept(bcDtlsServer, context.BcDtlsTransport);
            }
            else
            {
                context.BcDtlsTransport.SetStream(requestStream);
            }

            return Task.FromResult(0);
        }

        public string GetSigningCredentialSha256Fingerprint()
        {
            var sha256 = new SHA256Managed();
            var fingerprintBytes = sha256.ComputeHash(_signingCredentialCert.RawData);

            var fingerprint = string.Join(':', fingerprintBytes.Select(l => l.ToString("X2")));

            return fingerprint;
        }
    }
}
