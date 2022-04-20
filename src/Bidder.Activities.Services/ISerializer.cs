namespace Bidder.Activities.Services
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public interface ISerializer
    {
        public string Serialize(object data);
    }

    public class Serializer : ISerializer
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            Converters = { new JsonStringEnumConverter() }
        };

        public string Serialize(object data)
        {
            return JsonSerializer.Serialize(data, JsonSerializerOptions);
        }
    }
}