namespace Subspace.Rtp.Srtp
{
    public class SrtpStreamContext
    {
        public uint SynchronizationSource { get; set; }
        public int RollOverCounter { get; set; }
        public ushort PreviousSequenceNumber { get; set; }
        public uint KeyDerivationRate { get; set; } = SrtpConstants.DefaultKeyDerivationRate;
        public SrtpDerivedkeys DerivedKeys { get; set; }
    }
}
