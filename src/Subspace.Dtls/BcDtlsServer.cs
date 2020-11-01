using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Security;
using System;
using System.Collections;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using CertificateRequest = Org.BouncyCastle.Crypto.Tls.CertificateRequest;

namespace Subspace.Dtls
{
    internal class BcDtlsServer : DefaultTlsServer
    {
        private readonly X509CertificateStructure _cert;
        private readonly AsymmetricKeyParameter _key;
        public Action HandshakeComplete { get; set; }

        public BcDtlsServer(X509Certificate2 signingCert)
        {
            _cert = X509CertificateStructure.GetInstance(signingCert.RawData);

            var keyBytes = signingCert.GetRSAPrivateKey().ExportPkcs8PrivateKey();
            _key = PrivateKeyFactory.CreateKey(keyBytes);
        }

        protected override Org.BouncyCastle.Crypto.Tls.ProtocolVersion MinimumVersion => Org.BouncyCastle.Crypto.Tls.ProtocolVersion.DTLSv12;
        protected override Org.BouncyCastle.Crypto.Tls.ProtocolVersion MaximumVersion => Org.BouncyCastle.Crypto.Tls.ProtocolVersion.DTLSv12;


        public override IDictionary GetServerExtensions()
        {
            TlsSRTPUtils.AddUseSrtpExtension(CheckServerExtensions(), new UseSrtpData(
                new[] { Org.BouncyCastle.Crypto.Tls.SrtpProtectionProfile.SRTP_AES128_CM_HMAC_SHA1_80 },
                new byte[] { }));

            return base.GetServerExtensions();
        }

        protected override TlsSignerCredentials GetRsaSignerCredentials()
        {
            var alg = mSupportedSignatureAlgorithms.Cast<SignatureAndHashAlgorithm>().First(l => l.Signature == Org.BouncyCastle.Crypto.Tls.SignatureAlgorithm.rsa && l.Hash == HashAlgorithm.sha256);

            return new DefaultTlsSignerCredentials(mContext, new Certificate(new[] { _cert }), _key, alg);
        }

        public override CertificateRequest GetCertificateRequest()
        {
            byte[] certificateTypes = new byte[]{ ClientCertificateType.rsa_sign,
                ClientCertificateType.dss_sign, ClientCertificateType.ecdsa_sign };

            IList serverSigAlgs = null;
            if (TlsUtilities.IsSignatureAlgorithmsExtensionAllowed(mServerVersion))
            {
                serverSigAlgs = TlsUtilities.GetDefaultSupportedSignatureAlgorithms();
            }

            IList certificateAuthorities = new ArrayList();

            return new CertificateRequest(certificateTypes, serverSigAlgs, certificateAuthorities);
        }

        public override void NotifyClientCertificate(Certificate clientCertificate)
        {
        }
        public override void NotifyHandshakeComplete()
        {
            HandshakeComplete?.Invoke();
        }

        public byte[] GetKeyMaterial(int len)
        {
            if (mContext is null ||
                mContext.ServerVersion is null ||
                !mContext.SecurityParameters.IsExtendedMasterSecret ||
                mContext.SecurityParameters.MasterSecret is null)
            {
                return null;
            }

            return mContext.ExportKeyingMaterial(ExporterLabel.dtls_srtp, null, len);
        }
    }
}
