using Moq;
using OrdersMS.Core.Utils.Optional;
using OrdersMS.src.Contracts.Application.Queries.GetPolicyById;
using OrdersMS.src.Contracts.Application.Queries.GetPolicyById.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain.Entities;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using Xunit;

namespace OrdersMS.Tests.InsurancePolicies.Application.Queries
{
    public class GetPolicyByIdQueryHandlerTests
    {
        private readonly Mock<IPolicyRepository> _policyRepositoryMock;
        private readonly GetPolicyByIdQueryHandler _handler;

        public GetPolicyByIdQueryHandlerTests()
        {
            _policyRepositoryMock = new Mock<IPolicyRepository>();
            _handler = new GetPolicyByIdQueryHandler(_policyRepositoryMock.Object);
        }

        [Fact]
        public async Task GetInsurancePolicyByIdSuccess()
        {
            var query = new GetPolicyByIdQuery("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");
            var policy = new InsurancePolicy(
                new PolicyId(query.Id),
                new PolicyType("Diamante"),
                new PolicyCoverageKm(30),
                new PolicyIncidentCoverageAmount(55),
                new PriceExtraKm(2)
            );

            _policyRepositoryMock.Setup(x => x.GetById(query.Id)).ReturnsAsync(Optional<InsurancePolicy>.Of(policy));

            var result = await _handler.Execute(query);

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public async Task GetInsurancePolicyByIdWhenPolicyNotFound()
        {
            var query = new GetPolicyByIdQuery("53c0d8fa-dbca-4d98-9fdf-1d1413e90f9f");

            _policyRepositoryMock.Setup(x => x.GetById(query.Id)).ReturnsAsync(Optional<InsurancePolicy>.Empty());

            var result = await _handler.Execute(query);

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("Policy not found", result.ErrorMessage);
        }
    }
}
