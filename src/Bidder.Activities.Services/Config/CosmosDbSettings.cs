namespace Bidder.Activities.Services.Config;

public class CosmosDbSettings
{
    public string ConnectionString { get; set; }
    public string Endpoint { get; set; }
    public string MasterKey { get; set; }
    public string ContainerName { get; set; }
    public string Database { get; set; }
}