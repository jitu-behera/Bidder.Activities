using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bidder.Activities.Domain.Entities
{
    #region Service Registration Map
    public class ServiceBaseModel
    {
        [JsonPropertyName("domain_name")]
        public string DomainName { get; set; }

        [JsonPropertyName("service_name")]
        public string ServiceName { get; set; }
    }

    public class ServiceModel : ServiceBaseModel
    {
        [JsonPropertyName("id")]
        public string id { get; set; }

        [JsonPropertyName("platform")]
        public string Platform { get; set; }

        [JsonPropertyName("service_details")]
        public List<ServiceDetails> ServiceDetails { get; set; }

        [JsonPropertyName("created_date")]
        public string CreatedData { get; set; } = string.Empty;

        [JsonPropertyName("modified_date")]
        public string ModifiedDate { get; set; } = string.Empty;
    }

    public class ServiceDetails
    {
        [JsonPropertyName("service_description")]
        public string ServiceDescription { get; set; }

        [JsonPropertyName("service_access_type")]
        public string ServiceAccessType { get; set; }

        [JsonPropertyName("service_protocol_type")]
        public string ServiceProtocolType { get; set; }

        [JsonPropertyName("authentication_type")]
        public string AuthenticationType { get; set; }

        [JsonPropertyName("tags")]
        public string Tags { get; set; }

        [JsonPropertyName("is_third_party_service")]
        public string IsThirdPartyService { get; set; } = "No";

        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("address")]
        public ServicesAddress Address { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }

        [JsonPropertyName("stage")]
        public string Stage { get; set; }

        [JsonPropertyName("dependent_services")]
        public DependentServices[] DependentServices { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("health_status")]
        public string HealthStatus { get; set; }

        [JsonPropertyName("health_endpoint")]
        public string HealthEndpoint { get; set; }

        [JsonPropertyName("frequency")]
        public int Frequency { get; set; }
    }

    public class ServicesAddress
    {
        [JsonPropertyName("primary")]
        public string Primary { get; set; }

        [JsonPropertyName("fallback")]
        public string Fallback { get; set; }
    }

    public class DependentServices : ServiceBaseModel
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("health_status")]
        public string HealthStatus { get; set; }
    }

    public class ServiceHealthStatus : ServiceBaseModel
    {
        [JsonPropertyName("id")]
        public string id { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("health_status")]
        public string HealthStatus { get; set; }
    }

    public class ServiceResponse
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("msg")]
        public string Message { get; set; }

        [JsonPropertyName("data")]
        public dynamic Data { get; set; }

        [JsonPropertyName("is_from_cache")]
        public bool IsFromCache { get; set; }
    }

    #endregion

    #region Service Request Log
    public class ServiceRequest : ServiceBaseModel
    {
        [JsonPropertyName("id")]
        public string id { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("request_service_name")]
        public string RequestServiceName { get; set; }

        [JsonPropertyName("request_time")]
        public string RequestTime { get; set; }

        [JsonPropertyName("read_from_cache")]
        public bool ReadFromCache { get; set; } = true;
    }
    #endregion

    #region Service Dependency Map
    public class ServiceDependencyRegistration : ServiceBaseModel
    {
        public string id { get; set; }

        public string ServiceId { get; set; }

        public string Version { get; set; }

        public List<DependentServicesWithId> DependentServices { get; set; }
    }

    public class DependentServicesWithId : DependentServices
    {
        [JsonPropertyName("id")]
        public string ServiceId { get; set; }
    }



    public class ServiceHealthStatusUpdate : ServiceBaseModel
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("health_status")]
        public string HealthStatus { get; set; }

        [JsonPropertyName("region")]
        public string Region { get; set; }

        [JsonPropertyName("unhealthy_unregistered")]
        public string[] UnhealthyUnregistered { get; set; }

        [JsonPropertyName("is_service_updated")]
        public bool IsServiceUpdated { get; set; }

        [JsonPropertyName("is_unhealthy_flow")]
        public bool IsUnhealthyFlow { get; set; }
    }
    public class ParentServices : ServiceBaseModel
    {
        [JsonPropertyName("id")]
        public string id { get; set; }
        [JsonPropertyName("region")]
        public string Region { get; set; }
        [JsonPropertyName("dependent_services")]
        public DependentServices[] DependentServices { get; set; }
        [JsonPropertyName("version")]
        public string Version { get; set; }
        [JsonPropertyName("health_status")]
        public string HealthStatus { get; set; }
        [JsonPropertyName("health_endpoint")]
        public string HealthEndpoint { get; set; }
        [JsonPropertyName("address")]
        public ServicesAddress Address { get; set; }
    }

    #endregion
}
