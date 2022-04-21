using Bidder.Activities.Domain.Entities;
using Bidder.Activities.Domain.Validators.Fluent;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Bidder.Activities.Api.UnitTests
{
    public class RegistrationStatusValidatorTest
    {
        private readonly RegistrationStatusValidator _registrationStatusValidatorTest;

        public RegistrationStatusValidatorTest()
        {
            _registrationStatusValidatorTest = new RegistrationStatusValidator();
        }
        [Theory]
        [InlineData(11)]
        public void When_AuctionId_Is_Positive_Number_Should_Not_Have_Any_ValidationError(long auctionId)
        {
            var registrationStatusDomain = new RegistrationStatusDomain(_registrationStatusValidatorTest, auctionId);
            var result = _registrationStatusValidatorTest.TestValidate(registrationStatusDomain);
            result.IsValid.Should().Be(true);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void When_AuctionId_Is_Negative_Number_Should_Not_Have_Any_ValidationError(long auctionId)
        {
            var registrationStatusDomain = new RegistrationStatusDomain(_registrationStatusValidatorTest, auctionId);
            var result = _registrationStatusValidatorTest.TestValidate(registrationStatusDomain);
            result.IsValid.Should().Be(false);
        }


    }
}
