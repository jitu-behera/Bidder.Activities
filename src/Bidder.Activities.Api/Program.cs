using Bidder.Activities.Api.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

namespace Bidder.Activities.Api
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static void Main()
        {
            CreateHostBuilder().Build().Run();
        }

        private static IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                    webBuilder.ConfigureAzureAppConfiguration()
                        .ConfigureAppInsightsLogging()
                        .UseStartup<Startup>());
        }

        public class SwaggerHostFactory
        {
            public static IHost CreateHost()
            {
                return Host.CreateDefaultBuilder()
                                .ConfigureWebHostDefaults(webBuilder =>
                                    webBuilder
                                    .UseEnvironment("Development")
                                    .UseStartup<Startup>()).Build();
            }
        }
    }
}