namespace Subspace.Rtp.Srtp
{
    public class SrtpDerivedkeys
    {
        public byte[] SessionKey { get; set; }
        public byte[] CipherSalt { get; set; }
        public byte[] AuthKey { get; set; }
    }
}
