namespace Subspace.Rtp.Rtcp
{
    public class RtcpAppDefinedPacket : RtcpPacket
    {
        public const int HeaderLength = 8;

        public RtcpAppDefinedPacket()
            : base(RtcpPacketType.ApplicationDefined)
        {
        }

        public override ushort LengthIn32BitWordsMinusOne => (ushort)((HeaderLength + Name.Length + Data.Length) / 4 - 1);

        public uint SynchronizationSource { get; set; }

        public string Name { get; set; }

        public byte[] Data { get; set; }
    }
}
