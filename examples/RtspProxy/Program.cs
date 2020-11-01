using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Subspace.Rtp;
using Subspace.Rtsp;
using Subspace.WebRtc;
using System;
using System.Threading.Tasks;

namespace RtspProxy
{
    class Program
    {
        static IWebRtcServer _webRtcServer;

        static async Task Main(string[] args)
        {
            var webhost = WebHost
                .CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddWebRtcServer();

                    services.AddSingleton<IWebSocketSignalingServer, WebSocketSignalingServer>();
                    services.AddSingleton<IRtspClient, RtspClient>();
                    services.AddSingleton<IRtspPlayer, RtspPlayer>();
                    services.AddSingleton<IRtspProxyService, RtspProxyService>();

                    services.AddMvc();
                })
                .Configure(app =>
                {
                    app.UseDeveloperExceptionPage();

                    app.UseDefaultFiles();
                    app.UseStaticFiles();
                    app.UseWebSockets();

                    app.Use((ctx, next) => ConfigureWebSocketSignalingRoute(app, ctx, next));
                })
                .Build();

            _webRtcServer = webhost.Services.GetService<IWebRtcServer>();

            _ = Task.Run(_webRtcServer.RunAsync);
            await webhost.RunAsync();
        }

        static async Task ConfigureWebSocketSignalingRoute(IApplicationBuilder app, HttpContext context, Func<Task> next)
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                    var signalingServer = app.ApplicationServices.GetRequiredService<IWebSocketSignalingServer>();

                    await signalingServer.ProcessWebSocketAsync(context, webSocket);
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await next();
            }
        }
    }
}
