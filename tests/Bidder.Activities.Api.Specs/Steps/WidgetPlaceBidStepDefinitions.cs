using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Bidder.Activities.Api.Specs.Features;
using Bidder.Activities.Api.Specs.MockConfiguration;
using Bidder.Activities.Domain;
using Bidder.Activities.Domain.Entities;
using Bidder.Activities.Services.Services;
using FluentAssertions;
using Moq;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Bidder.Activities.Api.Specs
{
    [Binding]
    public class WidgetPlaceBidStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly CustomWebApplicationFactory<MockStartup> _factory;
        private readonly Mock<IRegistrationStatusRepository> _bidderStatusRepositoryMock;
        private readonly Mock<IBiddingService> _biddingServiceMock;
        private HttpResponseMessage GetResponse() { return _scenarioContext["Response"] as HttpResponseMessage; }
        public WidgetPlaceBidStepDefinitions(ScenarioContext scenarioContext, CustomWebApplicationFactory<MockStartup> factory)
        {
            _factory = factory;
            _scenarioContext = scenarioContext;
            _bidderStatusRepositoryMock = new Mock<IRegistrationStatusRepository>();
            _biddingServiceMock = new Mock<IBiddingService>();
        }

        [Given(@"there is a record for customer id (.*) and partitionKey (.*) in the database")]
        public void GivenThereIsARecordForCustomerIdA_Customer_AndMarketplaceUniqueCodeIdInTheDatabase(string documentKey, string partitionKey, Table table)
        {
            var registrationStatusDetails = table.CreateInstance<RegistrationStatus>();

            _bidderStatusRepositoryMock.Setup(x => x.GetItemAsync(documentKey, partitionKey))
                .ReturnsAsync(registrationStatusDetails);

            _scenarioContext["StatusRepositoryMock"] = _bidderStatusRepositoryMock;
        }

        [Given(@"there is no record with customer id (.*) and partitionKey (.*) in the database")]
        public void GivenThereIsNoRecordWithCustomerIdA_Customer_AndMarketplaceUniqueCodeInTheDatabase(string documentKey, string partitionKey)
        {
            _bidderStatusRepositoryMock.Setup(x => x.GetItemAsync(documentKey, partitionKey))
                .ReturnsAsync((RegistrationStatus)null);
            _scenarioContext["StatusRepositoryMock"] = _bidderStatusRepositoryMock;
        }

        [Given(@"the bidding service response code is (.*) with response body")]
        public void GivenTheBiddingServiceResponseIs(int statusCode, string multilineText)
        {
            _biddingServiceMock.Setup(x => x.PlaceBid(It.IsAny<BiddingRequest>()))
                .ReturnsAsync(new BiddingResponse()
                {
                    Response = multilineText,
                    StatusCode = (HttpStatusCode)statusCode
                });
        }

        private HttpClient GetClient(CustomWebApplicationFactory<MockStartup> factory)
        {
            var specsHttpClientFactory = new SpecsHttpClientFactory();
            specsHttpClientFactory.AddStub(_bidderStatusRepositoryMock.Object);
            specsHttpClientFactory.AddStub(_biddingServiceMock.Object);
            return specsHttpClientFactory.GetClient(factory, _scenarioContext);
        }

        [When(@"I send POST request to (.*)")]
        public async Task WhenISendPOSTRequestToVPlace_Bid(string url, string requestBody)
        {
            var httpClient = GetClient(_factory);
            _scenarioContext["Response"] = await httpClient.PostAsync(url, new StringContent(requestBody, Encoding.UTF8, "application/json"));
        }

        [Then(@"the response body should be")]
        public async Task ThenTheResponseBodyShouldBe(string expectedJson)
        {
            var actualJson = await GetResponse().Content.ReadAsStringAsync();
            actualJson.Should().Be(expectedJson);
        }

        [Then(@"the shared bidding service must have been called with")]
        public void ThenTheSharedBiddingServiceMustHaveBeenCalledWith(Table table)
        {
            var expected = table.CreateInstance<BiddingRequest>();

            _biddingServiceMock.Verify(x => x.PlaceBid(It.Is<BiddingRequest>(actual => AreEqual(actual, expected))), Times.Once);

        }

        private bool AreEqual(BiddingRequest actual, BiddingRequest expected)
        {
            actual.ItemId.Should().Be(expected.ItemId);
            actual.TenderId.Should().Be(expected.TenderId);
            actual.Amount.Should().Be(expected.Amount);
            actual.BuyerId.Should().Be(expected.BuyerId);
            actual.BuyerRef.Should().Be(expected.BuyerRef);
            actual.SourceId.Should().Be(expected.SourceId);
            actual.MarketplaceUniqueCode.Should().Be(expected.MarketplaceUniqueCode);
            actual.MarketplaceChannelCode.Should().Be(expected.MarketplaceChannelCode);
            return true;
        }

        [Then(@"the bidding service has never been called")]
        public void ThenTheBiddingServiceHasNeverBeenCalled()
        {
            _biddingServiceMock.Verify(x => x.PlaceBid(It.IsAny<BiddingRequest>()), Times.Never);
        }

    }
}
