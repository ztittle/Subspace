using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Subspace.WebRtc;

namespace Subspace.Tests
{
    [TestClass]
    public class WebRtcServerFactoryTests
    {
        [TestMethod]
        public void Create()
        {
            var services = new ServiceCollection();
            services.AddWebRtcServer();
            var server = services.BuildServiceProvider().GetRequiredService<IWebRtcServer>();

        }
    }
}
