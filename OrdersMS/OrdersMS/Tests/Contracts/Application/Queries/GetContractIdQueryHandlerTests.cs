using Moq;
using OrdersMS.src.Contracts.Application.Queries.GetContractId;
using OrdersMS.src.Contracts.Application.Queries.GetContractId.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using Xunit;

namespace OrdersMS.Tests.Contracts.Application.Queries
{
    public class GetContractIdQueryHandlerTests
    {
        private readonly Mock<IContractRepository> _contractRepositoryMock;
        private readonly GetContractIdQueryHandler _handler;

        public GetContractIdQueryHandlerTests()
        {
            _contractRepositoryMock = new Mock<IContractRepository>();
            _handler = new GetContractIdQueryHandler(_contractRepositoryMock.Object);
        }

        [Fact]
        public async Task GetContractIdSuccess()
        {
            var query = new GetContractIdQuery(6213);

            _contractRepositoryMock.Setup(x => x.GetContractIdByContractNumber(query.ContractNumber)).ReturnsAsync("53c0d8fa-dbca-4d98-9fdf-1d1413e90f7f");

            var result = await _handler.Execute(query);

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public async Task GetContractIdWhenContractNotFound()
        {
            var query = new GetContractIdQuery(6213);

            _contractRepositoryMock.Setup(x => x.GetContractIdByContractNumber(query.ContractNumber)).ReturnsAsync("");
            var result = await _handler.Execute(query);

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("Contract not found", result.ErrorMessage);
        }
    }
}
