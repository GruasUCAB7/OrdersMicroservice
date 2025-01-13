using Moq;
using OrdersMS.Core.Utils.Optional;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Commands.UpdateInsuredPolicy;
using OrdersMS.src.Contracts.Application.Commands.UpdateInsuredPolicy.Types;
using OrdersMS.src.Contracts.Application.Exceptions;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain.Entities;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using Xunit;

namespace OrdersMS.Tests.InsurancePolicies.Application.Commands
{
    public class UpdatePolicyCommandHandlerTests
    {
        private readonly Mock<IPolicyRepository> _policyRepositoryMock;
        private readonly UpdatePolicyCommandHandler _handler;

        public UpdatePolicyCommandHandlerTests()
        {
            _policyRepositoryMock = new Mock<IPolicyRepository>();
            _handler = new UpdatePolicyCommandHandler(_policyRepositoryMock.Object);
        }

        [Fact]
        public async Task ShouldUpdateInsurancePolicySucces()
        {
            var policyId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d";
            var command = new UpdatePolicyCommand(true, 30, 55, 2);

            var policy = new InsurancePolicy(
                new PolicyId(policyId),
                new PolicyType("Diamante"),
                new PolicyCoverageKm(30),
                new PolicyIncidentCoverageAmount(55),
                new PriceExtraKm(2)
            );

            _policyRepositoryMock.Setup(x => x.GetById(policyId)).ReturnsAsync(Optional<InsurancePolicy>.Of(policy));
            _policyRepositoryMock.Setup(x => x.Update(policy)).ReturnsAsync(Result<InsurancePolicy>.Success(policy));

            var result = await _handler.Execute((policyId, command));
            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.Equal(policyId, result.Unwrap().Id);
            Assert.Equal("Diamante", result.Unwrap().Type);
            Assert.Equal(30, result.Unwrap().CoverageKm);
            Assert.Equal(55, result.Unwrap().CoverageAmount);
            Assert.Equal(2, result.Unwrap().PriceExtraKm);
            Assert.True(result.Unwrap().IsActive);
        }

        [Fact]
        public async Task ShouldFailToUpdateInsurancePolicyWhenPolicyNotFound()
        {
            var policyId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d";
            var command = new UpdatePolicyCommand(true, 30, 55, 2);

            _policyRepositoryMock.Setup(x => x.GetById(policyId)).ReturnsAsync(Optional<InsurancePolicy>.Empty());

            var result = await _handler.Execute((policyId, command));

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("Policy not found", result.ErrorMessage);
        }

        [Fact]
        public async Task ShouldFailToUpdateInsurancePolicyWhenUpdateFails()
        {
            var policyId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d";
            var command = new UpdatePolicyCommand(true, 30, 55, 2);

            var policy = new InsurancePolicy(
                new PolicyId(policyId),
                new PolicyType("Diamante"),
                new PolicyCoverageKm(25),
                new PolicyIncidentCoverageAmount(50),
                new PriceExtraKm(3)
            );

            _policyRepositoryMock.Setup(x => x.GetById(policyId)).ReturnsAsync(Optional<InsurancePolicy>.Of(policy));
            _policyRepositoryMock.Setup(x => x.Update(policy)).ReturnsAsync(Result<InsurancePolicy>.Failure(new PolicyUpdateFailedException()));

            var result = await _handler.Execute((policyId, command));

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("The insurance policy could not be updated correctly", result.ErrorMessage);
        }
    }
}
