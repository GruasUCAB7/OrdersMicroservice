using Moq;
using OrdersMS.src.Contracts.Application.Queries.GetAllContracts;
using OrdersMS.src.Contracts.Application.Queries.GetAllContracts.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using Xunit;

namespace OrdersMS.Tests.Contracts.Application.Queries
{
    public class GetAllContractsQueryHandlerTests
    {
        private readonly Mock<IContractRepository> _contractRepositoryMock;
        private readonly GetAllContractsQueryHandler _handler;

        public GetAllContractsQueryHandlerTests()
        {
            _contractRepositoryMock = new Mock<IContractRepository>();
            _handler = new GetAllContractsQueryHandler(_contractRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllContractsSuccess()
        {
            var query = new GetAllContractsQuery(1, 5, "");
            var contracts = new List<Contract>
                {
                    Contract.CreateContract(
                        new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f7f"),
                        new ContractNumber(6235),
                        new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                        new VehicleId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5t")
                    ),
                    Contract.CreateContract(
                        new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f8f"),
                        new ContractNumber(6235),
                        new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5y"),
                        new VehicleId("53c0d8fa-dbca-4d98-9fdf-1d1413e90t2d")
                    )
            };

            _contractRepositoryMock.Setup(x => x.GetAll(query)).ReturnsAsync(contracts);
            var result = await _handler.Execute(query);

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.Equal(2, result.Unwrap().Length);
        }

        [Fact]
        public async Task GetAllContractsFailureWhenNoContractsFound()
        {
            var query = new GetAllContractsQuery(1, 5, "");

            _contractRepositoryMock.Setup(x => x.GetAll(query)).ReturnsAsync(new List<Contract>());

            var result = await _handler.Execute(query);

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.Empty(result.Unwrap());
        }
    }
}
