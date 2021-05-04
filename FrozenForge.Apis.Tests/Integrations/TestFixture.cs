using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Net.Http;

namespace FrozenForge.Apis.Tests.Integrations
{
    public class TestFixture
    {
        public TestFixture()
        {
            IWebHostBuilder builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TestStartup>()
                .ConfigureServices(sc => sc.Replace(ServiceDescriptor.Scoped(ctx => this.HttpClient)));

            this.TestServer = new TestServer(builder);

            this.HttpClient = this.TestServer.CreateClient();
        }

        public TestServer TestServer { get; }

        public HttpClient HttpClient { get; }
    }


}
