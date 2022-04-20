using Bidder.Activities.Api.Specs.Features;
using Bidder.Activities.Api.Specs.MockConfiguration;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using TechTalk.SpecFlow;

namespace Bidder.Activities.Api.Specs.Steps
{
    [Binding]
    public class HealthCheckSteps
    {
        private readonly HttpClient _client;
        private HttpResponseMessage _response;

        public HealthCheckSteps(CustomWebApplicationFactory<MockStartup> factory, ScenarioContext scenarioContext)
        {
            _client = new SpecsHttpClientFactory().GetClient(factory, scenarioContext);
        }

        [When(@"I ask for a service healthcheck")]
        public async Task WhenIAskForAServiceHealthcheck()
        {
            _response = await _client.GetAsync("/v1/health");
        }

        [Then(@"the response should be 200 OK")]
        public void ThenTheResponseShouldBeOK()
        {
            _response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
