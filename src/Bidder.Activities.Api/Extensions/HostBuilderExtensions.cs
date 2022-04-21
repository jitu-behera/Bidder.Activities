using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Bidder.Activities.Api.Extensions
{
    /// <summary>
    /// Extension methods on IWebHostBuilder.
    /// Other required extension methods for IWebHostBuilder can be created in this class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Register azure app configuration and key vault here.
        /// The keys configured here can be tightly coupled with any class and used via DI.
        /// The keys can also be used directly from IConfiguration properties dictionary.
        /// </summary>
        /// <param name="webHostBuilder"></param>
        internal static IWebHostBuilder ConfigureAzureAppConfiguration(this IWebHostBuilder webHostBuilder)
        {
            return webHostBuilder.ConfigureAppConfiguration((hostingContext, config) =>
            {
                var settings = config.Build();
                var appEndpoint = settings["AppConfigEndpoint"];
                var azureCred = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                {
                    ExcludeSharedTokenCacheCredential = true,
                    ManagedIdentityClientId = settings["AppObjectId"]
                });

                //config.AddAzureAppConfiguration(options =>
                //{
                //    options.Connect(new Uri(appEndpoint), azureCred)
                //        // Load configuration values with no label
                //        .Select(KeyFilter.Any, LabelFilter.Null)
                //        // Override with any configuration values specific to current hosting env
                //        .Select(KeyFilter.Any, hostingContext.HostingEnvironment.EnvironmentName)
                //        .ConfigureKeyVault(kv => kv.SetCredential(azureCred))
                //        .UseFeatureFlags(featureFlagOptions =>
                //        {
                //            // Flags based on labels to segregate different environments
                //            featureFlagOptions.Label = hostingContext.HostingEnvironment.EnvironmentName;
                //            featureFlagOptions.CacheExpirationInterval = TimeSpan.FromMinutes(1);
                //        })
                //        .ConfigureRefresh(refresh =>
                //        {
                //            // Configure a sentinel key which when changed, will force all the keys to be refreshed from Azure app configuration
                //            refresh.Register("AppConfigSentinel", true)
                //                .SetCacheExpiration(TimeSpan.FromMinutes(1));
                //        });
                //});
            });
        }

        /// <summary>
        /// Configure logging to azure application insights here.
        /// The 'AppInsightsInstrumentationKey' key comes directly from azure app config
        /// </summary>
        /// <param name="webHostBuilder"></param>
        /// <returns></returns>
        internal static IWebHostBuilder ConfigureAppInsightsLogging(this IWebHostBuilder webHostBuilder)
        {
            return webHostBuilder.ConfigureLogging((hostingContext, logging) =>
            {
                var key = hostingContext.Configuration["AppInsightsInstrumentationKey"];
                if (!string.IsNullOrEmpty(key))
                {
                    logging.AddApplicationInsights(key);

                    // Override default to Warning for everything starting with Microsoft and from Warning to Trace for everything else.
                    logging.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Warning);
                    logging.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Warning);

                    // Capture all log-level entries from Program
                    logging.AddFilter<ApplicationInsightsLoggerProvider>(typeof(Program).FullName, LogLevel.Warning);

                    // Capture all log-level entries from Startup
                    logging.AddFilter<ApplicationInsightsLoggerProvider>(typeof(Startup).FullName, LogLevel.Warning);
                }
            });
        }
    }
}