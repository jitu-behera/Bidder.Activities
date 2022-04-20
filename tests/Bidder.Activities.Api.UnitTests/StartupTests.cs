using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Xunit;

namespace Bidder.Activities.Api.UnitTests
{
    public class StartupTests
    {
        [Fact]
        public void ShouldBuildWithNoErrors()
        {
            var webHostBuilder = Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                    webBuilder
                        .UseDefaultServiceProvider((context, options) => options.ValidateOnBuild = true)
                        .UseEnvironment("Development")
                        .UseStartup<Startup>().UseSerilog());
            
            var webHost = webHostBuilder.Build();

            Assert.NotNull(webHost);
        }
    }
}
