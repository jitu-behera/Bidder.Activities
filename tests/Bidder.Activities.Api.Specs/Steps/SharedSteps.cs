using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Bidder.Activities.CorrelationId;
using Bidder.Activities.Domain;
using Bidder.Activities.Domain.Entities;
using Bidder.Activities.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace Bidder.Activities.Specs.Steps
{
    [Binding]
    public class SharedSteps
    {
        private readonly ScenarioContext _scenarioContext;
        private readonly Mock<Clock> _mockClock;
        private readonly Mock<ICorrelationIdProvider> _correlationIdProvider;

        private HttpResponseMessage GetResponse() { return _scenarioContext["Response"] as HttpResponseMessage; }


        public SharedSteps(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _mockClock = new Mock<Clock>();
            _correlationIdProvider = new Mock<ICorrelationIdProvider>();
        }

        [Given(@"my request body is")]
        public void GivenMyRequestBodyIs(string multilineText)
        {
            _scenarioContext["RequestBody"] = multilineText;
        }

        [Given(@"my required headers")]
        public void GivenMyRequiredHeaders(Table table)
        {
            var headers = table.CreateSet<Header>().ToList();
            _scenarioContext["Headers"] = headers;
        }

        [Given(@"the token is expired for")]
        public void GivenTheTokenIsExpiredFor(Table tokenInfo)
        {
            var expirationDate = DateTime.UtcNow.AddSeconds(1);
            var stringToken = MockTokenGenerator.GenerateTokenWith(new List<Claim>(), expirationDate);
            Thread.Sleep(2000);

            var headers = _scenarioContext["Headers"] as List<Header>;
            headers.Add(new Header() { Key = "Authorization", Value = $"Bearer {stringToken}" });
        }

        [Given(@"my authorization token header is authenticated with")]
        public void GivenTheUserIsAuthenticatedWith(Table tokenInfo)
        {
            var stringToken = GenerateValidTokenFrom(tokenInfo);

            var headers = _scenarioContext["Headers"] as List<Header>;
            headers.Add(new Header() { Key = "Authorization", Value = $"Bearer {stringToken}" });
        }

        private static string GenerateValidTokenFrom(Table tokenInfo)
        {
            var tokenDetails = tokenInfo.CreateInstance<TokenDetails>();
            var claims = new List<Claim>
            {
                new("platform_id", tokenDetails.SourceId),
                //new Claim("sub", model.Subject ?? string.Empty),
                new("ext_customer_id", tokenDetails.CustomerId ?? string.Empty),
                new("marketplace_Id", tokenDetails.MarketplaceUniqueCode ?? string.Empty),
            };

            return MockTokenGenerator.GenerateTokenWith(claims, DateTime.UtcNow.AddDays(1));
        }

        [Given(@"my authorization token cookie is authenticated with")]
        public void GivenMyAuthorizationTokenCookieIsAuthenticatedWith(Table tokenInfo)
        {
            var stringToken = GenerateValidTokenFrom(tokenInfo);

            var headers = _scenarioContext["Headers"] as List<Header>;
            headers.Add(new Header() { Key = "cookie", Value = $"roo_guid=a>>>>>>>>; ba_widget_auth_token={stringToken};roo_guid=a5818848-a5a6-4435-b30f-dac210e881a7;" });
        }


        [Given(@"the blob will be stored at (.*)")]
        public void GivenMyPersistedBlobIs(string uri)
        {
            _scenarioContext["BlobUri"] = uri;
        }

        [Then(@"response should be 200 OK")]
        public void ThenResponseShouldBeOK()
        {
            GetResponse().StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Then(@"response should be 202 Accepted")]
        public void ThenResponseShouldBeAccepted()
        {
            GetResponse().StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        [Then(@"response should be 401 Unauthorized")]
        public void ThenResponseShouldBeUnauthorized()
        {
            GetResponse().StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Then(@"response should be 400 Bad Request")]
        public void ThenResponseShouldBeBadRequest()
        {
            GetResponse().StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Then(@"response should be 403 Forbidden")]
        public void ThenResponseShouldBeForbidden()
        {
            GetResponse().StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Then(@"response should be 500 Internal Server error")]
        public void ThenResponseShouldBeInternalServerError()
        {
            GetResponse().StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Given(@"the current time is (.*)Z")]
        public void GivenTheCurrentTimeIs_Z(DateTime dateTime)
        {
            _mockClock.Setup(x => x.GetCurrentUtcDateTime()).Returns(dateTime);
            _scenarioContext.Add("MockClock", _mockClock);
        }

        [Given(@"the correlation id is ""(.*)""")]
        public void GivenTheCorrelationIdIs(string correlationId)
        {
            _correlationIdProvider.Setup(x => x.GetCorrelationId()).Returns(correlationId);
            _scenarioContext.Add("CorrelationId", _correlationIdProvider);
        }

        [StepArgumentTransformation]
        public long[] TransformToListOfLongs(string commaSeparatedList)
        {
            return commaSeparatedList.Split(",").Select(Int64.Parse).ToArray();
        }



        [Then(@"the response contains these validation errors with unique paths")]
        public async Task TheResponseShouldHaveTheseValidationErrorsWithUniquePaths(Table table)
        {
            var expectedValidationResults = table.CreateSet<ValidationError>().ToList();

            var jsonBody = await GetResponse().Content.ReadAsStringAsync();
            var responseBody = JsonConvert.DeserializeObject<ModelBindingValidationError>(jsonBody);

            var actualValidationResultsDictionary = responseBody.ValidationResults.ToDictionary(x => x.Path);

            foreach (var expected in expectedValidationResults)
            {
                actualValidationResultsDictionary.Should().ContainKey(expected.Path);

                var actual = actualValidationResultsDictionary[expected.Path];

                actual.Value.Should().Be(expected.Value);
                actual.Path.Should().Be(expected.Path);
                actual.Description.Should().Be(expected.Description);
                actual.Code.Should().Be(expected.Code);
            }
        }

    }

    public class Header
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

}
