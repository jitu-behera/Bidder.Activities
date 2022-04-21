using System.Net.Http;
using System.Threading.Tasks;
using Bidder.Activities.Api.Specs.Features;
using Bidder.Activities.Api.Specs.MockConfiguration;
using Bidder.Activities.Domain.Entities;
using Bidder.Activities.Services.Services;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Bidder.Activities.Api.Specs.Steps
{
    [Binding]
    public class BidderRegistrationStatusStepDefinitions
    {
        private readonly CustomWebApplicationFactory<MockStartup> _factory;
        private readonly ScenarioContext _scenarioContext;

        private RegistrationStatus _actualResponseBody;

        public BidderRegistrationStatusStepDefinitions(CustomWebApplicationFactory<MockStartup> factory, ScenarioContext scenarioContext)
        {
            _factory = factory;
            _scenarioContext = scenarioContext;
        }

        private HttpResponseMessage GetResponse() { return _scenarioContext["Response"] as HttpResponseMessage; }
        private HttpClient GetClient(CustomWebApplicationFactory<MockStartup> factory)
        {
            var specsHttpClientFactory = new SpecsHttpClientFactory();
            if (_scenarioContext.ContainsKey("StatusRepositoryMock"))
            {
                var mockRegistrationStatusRepository = _scenarioContext["StatusRepositoryMock"] as Mock<IRegistrationStatusRepository>;
                specsHttpClientFactory.AddStub(mockRegistrationStatusRepository.Object);
            }

            return specsHttpClientFactory.GetClient(factory, _scenarioContext);
        }

        private async Task GetRegistrationStatusResponse()
        {
            var jsonBody = await GetResponse().Content.ReadAsStringAsync();
            _actualResponseBody = JsonConvert.DeserializeObject<RegistrationStatus>(jsonBody);
        }


        [When(@"I send get request to (.*)")]
        public async Task WhenISendGetRequestTo(string endpoint)
        {

            var httpClient = GetClient(_factory);
            var httpResponseMessage = await httpClient.GetAsync("/v1" + endpoint);
            _scenarioContext["Response"] = httpResponseMessage;
        }

        [Then(@"the response should contain these details")]
        public void ThenTheResponseShouldContainTheseDetails(Table responseDetails)
        {
            var registrationStatus = responseDetails.CreateInstance<RegistrationStatus>();
            _ = GetRegistrationStatusResponse();

            _actualResponseBody.BuyerId?.Should().Be(registrationStatus.BuyerId);
            _actualResponseBody.CTA?.Should().Be(registrationStatus.CTA);
            _actualResponseBody.Status.Should().Be(registrationStatus.Status);
        }

    }
}
