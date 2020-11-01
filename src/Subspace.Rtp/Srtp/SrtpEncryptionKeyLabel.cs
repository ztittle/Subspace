namespace Subspace.Rtp.Srtp
{
    public enum SrtpEncryptionKeyLabel : byte
    {
        SrtpEncryptionKey = 0,
        SrtpMessageAuthenticationKey = 1,
        SrtpSaltingKey = 2,
        SrtcpEncryptionKey = 3,
        SrtcpMessageAuthenticationKey = 4,
        SrtcpSaltingKey = 5
    }
}
