namespace Subspace.Rtp.Srtp
{
    public class SrtpConstants
    {
        public const int SrtpDefaultMasterKeyKeyLength = 128 / 8;
        public const int SrtpDefaultMasterKeySaltLength = 112 / 8;

        public const int SrtpDefaultEncryptionSessionKeyLength = 128 / 8;
        public const int SrtpDefaultAuthSessionKeyLength = 160 / 8;
        public const int SrtpDefaultSaltSessionKeyLength = 112 / 8;

        public const int DefaultKeyDerivationRate = 0;

        /// <summary>
        /// For the purpose of key derivation in SRTP, a secure PRF with
        /// m = 128 (or more) MUST be used
        /// </summary>
        public const int SrtpPseudoRandomFunctionInputBlockSize = 128 / 8;
    }
}
