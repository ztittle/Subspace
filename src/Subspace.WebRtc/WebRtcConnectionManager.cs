using Subspace.Dtls;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Timers;

namespace Subspace.WebRtc
{
    public interface IWebRtcConnectionManager
    {
        PeerConnection Get(IPEndPoint remoteEndpoint);
        PeerConnection GetOrAdd(IPEndPoint remoteEndpoint);
        IReadOnlyCollection<PeerConnection> GetAll();
    }

    public class WebRtcConnectionManager : IWebRtcConnectionManager
    {
        private readonly ConcurrentDictionary<IPEndPoint, PeerConnection> _connections = new ConcurrentDictionary<IPEndPoint, PeerConnection>();
        private readonly Timer _timer;
        private readonly TimeSpan _clientReceiveTimeout = TimeSpan.FromSeconds(10);

        public WebRtcConnectionManager()
        {
            _timer = new Timer(1000);
            _timer.Elapsed += RemoveStaleClients;
            _timer.Start();
        }

        private void RemoveStaleClients(object sender, ElapsedEventArgs e)
        {
            var utcNow = DateTime.UtcNow;

            var connections = _connections.ToArray();

            foreach (var (ipEndpoint, conn) in connections)
            {
                if (utcNow - conn.LastReceiveTimestamp > _clientReceiveTimeout)
                {
                    Debug.WriteLine($"Removing connection {ipEndpoint}. No data received within {_clientReceiveTimeout.TotalSeconds} seconds.", nameof(WebRtcConnectionManager));
                    RemoveClient(ipEndpoint);
                }
            }
        }

        public void RemoveClient(IPEndPoint clientEndpoint)
        {
            _connections.TryRemove(clientEndpoint, out _);
        }

        public PeerConnection GetOrAdd(IPEndPoint remoteEndpoint)
        {
            return _connections.GetOrAdd(remoteEndpoint, k =>
            {
                Debug.WriteLine($"Client {k} connected", nameof(WebRtcConnectionManager));
                return new PeerConnection
                {
                    IpRemoteEndpoint = remoteEndpoint,
                    DtlsContext = new DtlsContext()
                };
            });
        }

        public PeerConnection Get(IPEndPoint remoteEndpoint)
        {
            _connections.TryGetValue(remoteEndpoint, out var connection);

            return connection;
        }

        public IReadOnlyCollection<PeerConnection> GetAll()
        {
            return _connections.Values.ToList();
        }
    }
}
