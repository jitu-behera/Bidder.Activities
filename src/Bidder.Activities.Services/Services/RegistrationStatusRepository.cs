using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Bidder.Activities.Domain.Entities;
using Bidder.Activities.Services.Config;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.Cosmos;
namespace Bidder.Activities.Services.Services
{
    public interface IRegistrationStatusRepository
    {
        Task<RegistrationStatus> GetItemAsync(string id, string partitionKey);
    }

    public class RegistrationStatusRepository : IRegistrationStatusRepository
    {
        private const string DependencyTypeName = "Azure DocumentDB";
        private const string DependencyName = "BidderRegistrationSelect";
        private CosmosClient _cosmosClient;
        private readonly CosmosDbSettings _cosmosDbSettings;
        private readonly TelemetryClient _telemetryClient;
        private volatile Container _container;
        private static readonly object LockObject = new();

        private static readonly CosmosClientOptions CosmosClientOptions = new()
        {
            MaxRetryAttemptsOnRateLimitedRequests = 2,
            MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(1),
            ConsistencyLevel = ConsistencyLevel.Eventual,
            RequestTimeout = TimeSpan.FromMilliseconds(3000),
            SerializerOptions = new CosmosSerializationOptions
            { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase }
        };

        public RegistrationStatusRepository(CosmosDbSettings cosmosDbSettings, TelemetryClient telemetryClient)
        {
            _cosmosDbSettings = cosmosDbSettings;
            _telemetryClient = telemetryClient;
        }

        public async Task<RegistrationStatus> GetItemAsync(string id, string partitionKey)
        {
            var startTime = DateTime.UtcNow;
            var timer = Stopwatch.StartNew();
            Exception exception = null;
            try
            {
                return await GetContainer().ReadItemAsync<RegistrationStatus>(id, new PartitionKey(partitionKey)).ConfigureAwait(false);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                exception = ex;
                return new RegistrationStatus();
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                timer.Stop();
                _telemetryClient.TrackDependency(new DependencyTelemetry(DependencyTypeName, null, DependencyName, id, startTime, timer.Elapsed, "", exception == null));
            }
        }
        private Container GetContainer()
        {
            if (_container != null)
                return _container;

            lock (LockObject)
            {
                if (_container != null)
                    return _container;

                var connectionString = _cosmosDbSettings.ConnectionString;
                _cosmosClient = new CosmosClient(connectionString, CosmosClientOptions);

                var databaseId = _cosmosDbSettings.Database;
                var containerName = _cosmosDbSettings.ContainerName;

                var database = _cosmosClient.GetDatabase(databaseId);
                _container = database.GetContainer(containerName);
            }
            return _container;
        }
    }
}
