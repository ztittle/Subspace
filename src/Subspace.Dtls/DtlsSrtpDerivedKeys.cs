using System.IO;

namespace Subspace.Dtls
{
    /// <summary>
    /// When SRTP mode is in effect, different keys are used for ordinary
    /// DTLS record protection and SRTP packet protection.  These keys are
    /// generated using a TLS exporter [RFC5705] to generate
    /// 
    /// 2 * (SRTPSecurityParams.master_key_len +
    /// SRTPSecurityParams.master_salt_len) bytes of data
    /// 
    /// which are assigned as shown below.  The per-association context value
    /// is empty.
    /// 
    /// client_write_SRTP_master_key[SRTPSecurityParams.master_key_len];
    /// server_write_SRTP_master_key[SRTPSecurityParams.master_key_len];
    /// client_write_SRTP_master_salt[SRTPSecurityParams.master_salt_len];
    /// server_write_SRTP_master_salt[SRTPSecurityParams.master_salt_len];
    /// 
    /// https://tools.ietf.org/html/rfc5764#section-4.2
    /// </summary>
    public class DtlsSrtpDerivedKeys
    {
        public DtlsSrtpDerivedKeys(byte[] dtlsKeyMaterial, int keyLength, int saltLength)
        {
            using var ms = new MemoryStream(dtlsKeyMaterial);
            using var br = new BinaryReader(ms);

            ClientKey = br.ReadBytes(keyLength);
            ServerKey = br.ReadBytes(keyLength);

            ClientSalt = br.ReadBytes(saltLength);
            ServerSalt = br.ReadBytes(saltLength);
        }

        public byte[] ClientKey { get; }
        public byte[] ClientSalt { get; }
        public byte[] ServerKey { get; }
        public byte[] ServerSalt { get; }
    }
}
