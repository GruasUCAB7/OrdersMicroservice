using Moq;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Commands.CreateInsurancePolicy;
using OrdersMS.src.Contracts.Application.Commands.CreateInsurancePolicy.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain.Entities;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using Xunit;

namespace OrdersMS.Tests.InsurancePolicies.Application.Commands
{
    public class CreatePolicyCommandHandlerTests
    {
        private readonly Mock<IPolicyRepository> _policyRepositoryMock;
        private readonly Mock<IdGenerator<string>> _idGeneratorMock;
        private readonly CreatePolicyCommandHandler _handler;

        public CreatePolicyCommandHandlerTests()
        {
            _policyRepositoryMock = new Mock<IPolicyRepository>();
            _idGeneratorMock = new Mock<IdGenerator<string>>();
            _handler = new CreatePolicyCommandHandler(_policyRepositoryMock.Object, _idGeneratorMock.Object);
        }

        [Fact]
        public async Task ShouldCreateInsuredVehicleSuccess()
        {
            var command = new CreatePolicyCommand("Diamante", 25, 50, 3);

            _policyRepositoryMock.Setup(x => x.ExistByType(command.Type)).ReturnsAsync(false);
            _idGeneratorMock.Setup(x => x.Generate()).Returns("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d");
            var result = await _handler.Execute(command);

            var policy = new InsurancePolicy(
                new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new PolicyType(command.Type),
                new PolicyCoverageKm(command.CoverageKm),
                new PolicyIncidentCoverageAmount(command.CoverageAmount),
                new PriceExtraKm(command.PriceExtraKm)
            );

            _policyRepositoryMock.Setup(x => x.Save(policy)).ReturnsAsync(Result<InsurancePolicy>.Success(policy));

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public async Task ShouldFailToCreateInsurancePolicy_WhenInsurancePolicyAlreadyExists()
        {
            var command = new CreatePolicyCommand("Diamante", 25, 50, 3);

            _policyRepositoryMock.Setup(x => x.ExistByType(command.Type)).ReturnsAsync(true);
            var result = await _handler.Execute(command);

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal($"Insurance policy with type Diamante already exist", result.ErrorMessage);
        }
    }
}