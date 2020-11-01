using System;
using System.Net.Sockets;

namespace Subspace.WebRtc
{
    public static class SocketExtensions
    {
        public const int SIO_UDP_CONNRESET = -1744830452;

        internal static void IgnoreConnectionReset(this Socket socket)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                return;
            }

            // https://stackoverflow.com/questions/5199026/c-sharp-async-udp-listener-socketexception
            socket.IOControl(
                (IOControlCode)SIO_UDP_CONNRESET,
                new byte[] { 0, 0, 0, 0 },
                null
            );
        }
    }
}
