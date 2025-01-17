using Moq;
using OrdersMS.Core.Utils.Optional;
using OrdersMS.src.Contracts.Application.Queries.GetContractById;
using OrdersMS.src.Contracts.Application.Queries.GetContractById.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using Xunit;

namespace OrdersMS.Tests.Contracts.Application.Queries
{
    public class GetContractByIdQueryHandlerTests
    {
        private readonly Mock<IContractRepository> _contractRepositoryMock;
        private readonly GetContractByIdQueryHandler _handler;

        public GetContractByIdQueryHandlerTests()
        {
            _contractRepositoryMock = new Mock<IContractRepository>();
            _handler = new GetContractByIdQueryHandler(_contractRepositoryMock.Object);
        }

        [Fact]
        public async Task GetContractByIdSuccess()
        {
            var query = new GetContractByIdQuery("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");
            var contract = Contract.CreateContract(
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f7f"),
                new ContractNumber(6235),
                new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new VehicleId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5t"),
                DateTime.UtcNow
            );

            _contractRepositoryMock.Setup(x => x.GetById(query.Id)).ReturnsAsync(Optional<Contract>.Of(contract));

            var result = await _handler.Execute(query);

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public async Task GetContractByIdWhenContractNotFound()
        {
            var query = new GetContractByIdQuery("53c0d8fa-dbca-4d98-9fdf-1d1413e90f9f");

            _contractRepositoryMock.Setup(x => x.GetById(query.Id)).ReturnsAsync(Optional<Contract>.Empty());

            var result = await _handler.Execute(query);

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("Contract not found", result.ErrorMessage);
        }
    }
}
