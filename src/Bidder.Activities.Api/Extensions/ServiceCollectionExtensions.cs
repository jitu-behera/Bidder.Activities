using Bidder.Activities.Api.Application.Policies;
using Bidder.Activities.Api.Services;
using Bidder.Activities.Domain;
using Bidder.Activities.Services;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using System.Diagnostics.CodeAnalysis;
using System.IO.Compression;
using System.Net.Http;
using Bidder.Activities.Api.Domain.Shared;
using Bidder.Activities.Domain.Validators.Fluent;
using Bidder.Activities.Services.Config;
using Bidder.Activities.Services.Services;
using Azure.Identity;
using Bidder.Activities.CorrelationId;
using Bidder.Activities.HttpHeaders;
using FluentValidation;
using RequestResponseLogger.Configuration;

namespace Bidder.Activities.Api.Extensions
{
    /// <summary>
    /// Extension methods on IServiceCollection.
    /// Other required extension methods for IServiceCollection can be created in this class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register typed HttpClients along with all policies pre-configured using this extension method.
        /// Http clients can then be directly injected into the typed implementation.
        /// Other such extension methods can be created for HttpClient if required.
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services">Service collection</param>
        internal static void AddTypeImplementedHttpClient<TClient, TImplementation>(this IServiceCollection services)
            where TClient : class
            where TImplementation : class, TClient
        {
            services
                .AddHttpClient<TClient, TImplementation>()
                .AddPolicyHandler(request => request.Method == HttpMethod.Get
                    ? Application.Policies.Polly.GetWaitAndRetryPolicy()
                    : Application.Policies.Polly.GetNoOperationPolicy())
                .AddPolicyHandler(Application.Policies.Polly.GetCircuitBreakerPolicy())
                .AddPolicyHandler(Application.Policies.Polly.GetTimeoutPolicy());
        }

        /// <summary>
        /// Register api versioning here.
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="majorVersion">Major version of api</param>
        internal static void AddApiVersioning(this IServiceCollection services, int majorVersion)
        {
            services.AddApiVersioning(c =>
            {
                c.DefaultApiVersion = new ApiVersion(majorVersion, 0);
                c.AssumeDefaultVersionWhenUnspecified = true;
                c.ReportApiVersions = true;
                c.ApiVersionReader = new UrlSegmentApiVersionReader();
            });
        }

        /// <summary>
        /// Register all the pipeline behaviors here
        /// </summary>
        /// <param name="services"></param>
        internal static void AddPipelineBehaviors(this IServiceCollection services)
        {

            //services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        }

        /// <summary>
        /// Register all external/third party services i.e. Azure here.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        internal static void AddExternalServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAzureAppConfiguration();
            services.AddFeatureManagement();
            services.AddApplicationInsightsTelemetry(configuration);
            services.AddApplicationInsightsKubernetesEnricher();
            services.AddSingleton<ITelemetryInitializer, TelemetryInitializer>();
            services.AddCosmosDbSettings(configuration);
        }
        internal static void AddCosmosDbSettings(this IServiceCollection services, IConfiguration configuration)
        {
            var cosmosConfiguration = new CosmosDbSettings();
            configuration.Bind("CosmosDbSettings", cosmosConfiguration);
            services.AddSingleton(cosmosConfiguration);
        }
        /// <summary>
        /// Register all internal dependencies i.e helpers, wrappers, etc. here.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        internal static void AddInternalServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            services.AddMemoryCache();

            services.AddValidators();

            var requiredHeaders = new RequiredHeaders();
            configuration.Bind("Headers", requiredHeaders);
            services.AddSingleton(requiredHeaders);

            services.AddSingleton<Clock>();

            services.AddSingleton<ISerializer, Serializer>();

            services.AddSingleton<ITelemetryLogger, TelemetryLogger>();

            services.AddSingleton<IRegistrationStatusRepository, RegistrationStatusRepository>();
            services.AddSingleton<ITokenService, TokenService>();
            services.AddSingleton<RegistrationStatusService>();

            var biddingServiceConfigs = new BiddingServiceConfigs();
            configuration.Bind("BiddingServiceConfigs", biddingServiceConfigs);
            services.AddSingleton(biddingServiceConfigs);

            // services.AddSingleton<IBiddingService, BiddingService>();
            services.AddSingleton<IBiddingService, MockBiddingService>();
            
        }
        public static void AddCorrelationIdServices(this IServiceCollection serviceCollection, CorrelationIdConfiguration configuration)
        {
            serviceCollection.AddSingleton(typeof(ICorrelationIdProvider), typeof(CorrelationIdProvider));
            serviceCollection.AddSingleton(typeof(ICorrelationIdManager), typeof(CorrelationIdProvider));
            serviceCollection.AddSingleton(configuration);
        }
        public static void AddLoggingProvider(this IServiceCollection serviceCollection, LoggingConfiguration loggingConfig)
        {
            serviceCollection.AddSingleton(typeof(ICorrelationIdProvider), typeof(CorrelationIdProvider));
            serviceCollection.AddSingleton(typeof(IRequestResponseLogger), typeof(RequestResponseLogger.RequestResponseLogger));
            serviceCollection.AddSingleton(loggingConfig);
        }
        internal static void AddValidators(this IServiceCollection services)
        {
            services.AddValidatorsFromAssemblies(new[] { typeof(RegistrationStatusValidator).Assembly }, ServiceLifetime.Singleton);
        }

        /// <summary>
        /// Map configuration related to AppSettings, Azure AppConfig, etc. here.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        internal static void MapConfigurationToTypedClass(this IServiceCollection services, IConfiguration configuration)
        {
            // Maps app config values from a particular section to a class which can be then injected anywhere using IOptions<>.
            services.Configure<Headers>(configuration.GetSection(nameof(Headers)));

            // Bind app config values in case of static classes.
            configuration.GetSection(nameof(PollySettings)).Bind(Application.Policies.Polly.PollySettings);
        }

        /// <summary>
        /// Configure Gzip response compression here
        /// </summary>
        /// <param name="services"></param>
        internal static void AddGzipCompression(this IServiceCollection services)
        {
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
            });

            services.Configure<GzipCompressionProviderOptions>(options => { options.Level = CompressionLevel.Optimal; });
        }

        internal static void AddAzureServices(this IServiceCollection services, IConfiguration config)
        {
            services.AddAzureClients(builder =>
            {
                var busNameSpace = config.GetValue<string>("ServiceBusConfiguration:ServiceBusNameSpace");
                var blobAccount = config.GetValue<string>("BlobConfiguration:BlobAccountName");
                //builder.AddServiceBusClientWithNamespace(busNameSpace);
                //builder.AddBlobServiceClient(new Uri($"https://{blobAccount}.blob.core.windows.net"));
                builder.UseCredential(new DefaultAzureCredential(new DefaultAzureCredentialOptions()
                {
                    ExcludeSharedTokenCacheCredential = true,
                    ManagedIdentityClientId = config["AppObjectId"]
                }));
            });
        }
    }
}