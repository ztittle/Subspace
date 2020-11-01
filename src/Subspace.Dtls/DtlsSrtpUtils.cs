using Subspace.Rtp.Srtp;

namespace Subspace.Dtls
{
    public class DtlsSrtpUtils
    {
        public static SrtpPolicy CreateSrtpPolicy(byte[] dtlsKeyMaterial)
        {
            var derivedKeys = new DtlsSrtpDerivedKeys(dtlsKeyMaterial, SrtpConstants.SrtpDefaultMasterKeyKeyLength, SrtpConstants.SrtpDefaultMasterKeySaltLength);

            var srtpPolicy = new SrtpPolicy
            {
                MasterKey = derivedKeys.ServerKey,
                MasterSalt = derivedKeys.ServerSalt
            };

            return srtpPolicy;
        }
    }
}
