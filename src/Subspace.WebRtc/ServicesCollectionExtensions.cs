using Subspace.Dtls;
using Subspace.Rtp;
using Subspace.Rtp.Rtcp;
using Subspace.Stun;
using Subspace.WebRtc;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServicesCollectionExtensions
    {
        public static IServiceCollection AddWebRtcServer(this IServiceCollection services, Action<WebRtcServerOptions> opt = null)
        {
            if (opt != null) services.Configure(opt);
            services.AddSingleton<IWebRtcServer, WebRtcServer>();

            services.AddSingleton<IWebRtcConnectionManager, WebRtcConnectionManager>();
            services.AddSingleton<IStunHandler, StunHandler>();
            services.AddSingleton<IStunUserProvider, IceInMemoryStunUserProvider>();
            services.AddSingleton<IDtlsHandler, DtlsHandler>();
            services.AddSingleton<IRtpHandler, RtpHandler>();
            services.AddSingleton<IDtlSrtpMultiplexer, DtlsSrtpMultiplexer>();
            services.AddSingleton<IRtpClient, RtpClient>();
            services.AddSingleton<IRtcpClient, RtcpClient>();
            services.AddSingleton<IRtcpReceptionReportScheduler, RtcpReceptionReportScheduler>();

            return services;
        }
    }
}
