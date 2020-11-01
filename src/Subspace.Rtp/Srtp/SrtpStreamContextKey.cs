using System;
using System.Net;

namespace Subspace.Rtp.Srtp
{
    public readonly struct SrtpStreamContextKey : IEquatable<SrtpStreamContextKey>
    {
        public SrtpStreamContextKey(uint synchronizationSource, IPEndPoint remoteEndPoint)
            => (SynchronizationSource, RemoteEndPoint) = (synchronizationSource, remoteEndPoint);

        public uint SynchronizationSource { get; }

        public IPEndPoint RemoteEndPoint { get; }

        public override int GetHashCode() => (SynchronizationSource, RemoteEndPoint).GetHashCode();
        public override bool Equals(object obj) => obj is SrtpStreamContextKey key && Equals(key);

        public bool Equals(SrtpStreamContextKey other) =>
            SynchronizationSource.Equals(other.SynchronizationSource) && RemoteEndPoint.Equals(other.RemoteEndPoint);

        public static bool operator ==(SrtpStreamContextKey left, SrtpStreamContextKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SrtpStreamContextKey left, SrtpStreamContextKey right)
        {
            return !(left == right);
        }
    }
}
