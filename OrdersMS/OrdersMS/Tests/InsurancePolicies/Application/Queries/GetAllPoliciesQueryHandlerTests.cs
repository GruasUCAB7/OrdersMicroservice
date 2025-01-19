using Moq;
using OrdersMS.src.Contracts.Application.Queries.GetAllPolicies;
using OrdersMS.src.Contracts.Application.Queries.GetAllPolicies.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain.Entities;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using Xunit;


namespace OrdersMS.Tests.InsurancePolicies.Application.Queries
{
    public class GetAllPoliciesQueryHandlerTests
    {
        private readonly Mock<IPolicyRepository> _policyRepositoryMock;
        private readonly GetAllPoliciesQueryHandler _handler;

        public GetAllPoliciesQueryHandlerTests()
        {
            _policyRepositoryMock = new Mock<IPolicyRepository>();
            _handler = new GetAllPoliciesQueryHandler(_policyRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllInsurancePoliciesSuccess()
        {
            var query = new GetAllPoliciesQuery(1, 5, "");
            var policies = new List<InsurancePolicy>
                {
                    new InsurancePolicy(
                        new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f2h"),
                        new PolicyType("Diamante"),
                        new PolicyCoverageKm(30),
                        new PolicyIncidentCoverageAmount(55),
                        new PriceExtraKm(2)
                    ),
                    new InsurancePolicy(
                        new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90ffr"),
                        new PolicyType("Premium"),
                        new PolicyCoverageKm(40),
                        new PolicyIncidentCoverageAmount(80),
                        new PriceExtraKm(3)
                    )
            };


            _policyRepositoryMock.Setup(x => x.GetAll(query)).ReturnsAsync(policies);
            var result = await _handler.Execute(query);

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.Equal(2, result.Unwrap().Length);
        }

        [Fact]
        public async Task GetAllInsurancePoliciesFailureWhenNoPoliciesFound()
        {
            var query = new GetAllPoliciesQuery(1, 5, "");

            _policyRepositoryMock.Setup(x => x.GetAll(query)).ReturnsAsync(new List<InsurancePolicy>());

            var result = await _handler.Execute(query);

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.Empty(result.Unwrap());
        }
    }
}
