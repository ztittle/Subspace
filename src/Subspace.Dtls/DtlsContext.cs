namespace Subspace.Dtls
{
    public class DtlsContext
    {
        internal BcDtlsTransport BcDtlsTransport { get; set; }
        internal BcDtlsServer BcDtlsServer { get; set; }
        public byte[] DtlsKeyMaterial { get; internal set; }
    }
}
