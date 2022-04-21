using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Bidder.Activities.Domain;
using Bidder.Activities.Services.Config;
using Microsoft.AspNetCore.Hosting;

namespace Bidder.Activities.Services.Services;

public class BiddingService : IBiddingService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly BiddingServiceConfigs _biddingServiceConfigs;
    private static string _endpoint;

    public BiddingService(IWebHostEnvironment webHostEnvironment, BiddingServiceConfigs biddingServiceConfigs)
    {
        _webHostEnvironment = webHostEnvironment;
        _biddingServiceConfigs = biddingServiceConfigs;

        if (string.Equals(_webHostEnvironment.EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase))
        {
            _endpoint = biddingServiceConfigs.DevEndpoint;
        }
        else
        {
            _endpoint = biddingServiceConfigs.ClusterEndpoint;
        }
    }

    public Task<BiddingResponse> PlaceBid(BiddingRequest biddingRequest)
    {
        return ForwardBid(biddingRequest);
    }

    public async Task<BiddingResponse> ForwardBid(BiddingRequest placeBidIn)
    {
        var bodyJson = JsonSerializer.Serialize(placeBidIn);

        var httpContent = new StringContent(bodyJson, System.Text.Encoding.UTF8, "application/json");

        using HttpResponseMessage result = await ClientWithHeaders.PostAsync(_endpoint, httpContent).ConfigureAwait(false);
        var response = await result.Content.ReadAsStringAsync().ConfigureAwait(false);

        return new BiddingResponse { StatusCode = result.StatusCode, Response = response };
    }

    private HttpClient _httpClient = null;
    private HttpClient ClientWithHeaders
    {
        get
        {
            if (_httpClient == null)
            {
                var client = new HttpClient();
                client.DefaultRequestHeaders.Add("x-ba-correlation-id", "1");
                client.DefaultRequestHeaders.Add("x-ba-client-id", "1");
                client.DefaultRequestHeaders.Add("x-ba-client-ip", "1");
                client.DefaultRequestHeaders.Add("x-ba-app-id", "1");
                client.DefaultRequestHeaders.Add("x-ba-user-id", "1");
                client.DefaultRequestHeaders.Add("x-ba-rm", "std");
                client.DefaultRequestHeaders.Add("ocp-apim-subscription-key", _biddingServiceConfigs.APIMKey);
                _httpClient = client;
            }
            return _httpClient;
        }
    }
}

public class MockBiddingService : IBiddingService
{
    public Task<BiddingResponse> PlaceBid(BiddingRequest biddingRequest)
    {
        return Task.FromResult(new BiddingResponse() { StatusCode = HttpStatusCode.OK, Response = System.Text.Json.JsonSerializer.Serialize(new {success="true"}) });
    }
}