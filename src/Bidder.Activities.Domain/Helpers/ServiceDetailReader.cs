using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using Bidder.Activities.Domain.Entities;

namespace Bidder.Activities.Domain.Helpers
{
    /// <summary>
    /// Static class to access all the details related to the current micro service.
    /// This class reads all the details from a file at the root of the solution -> ServiceDetails.json
    /// Changes should be made in this file to input details of the current micro service.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ServiceDetailReader
    {
        static ServiceDetailReader()
        {
            ServiceModel = JsonSerializer.Deserialize<ServiceModel>(File.ReadAllText("ServiceDetails.json"));
        }

        public static ServiceModel ServiceModel { get; }
    }
}
