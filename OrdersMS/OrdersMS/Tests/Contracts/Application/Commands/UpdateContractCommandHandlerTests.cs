using Moq;
using OrdersMS.Core.Utils.Optional;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Commands.UpdateContract;
using OrdersMS.src.Contracts.Application.Commands.UpdateContract.Types;
using OrdersMS.src.Contracts.Application.Exceptions;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using Xunit;

namespace OrdersMS.Tests.Contracts.Application.Commands
{
    public class UpdateContractCommandHandlerTests
    {
        private readonly Mock<IContractRepository> _contractRepositoryMock;
        private readonly UpdateContractCommandHandler _handler;

        public UpdateContractCommandHandlerTests()
        {
            _contractRepositoryMock = new Mock<IContractRepository>();
            _handler = new UpdateContractCommandHandler(_contractRepositoryMock.Object);
        }

        [Fact]
        public async Task ShouldUpdateContractSucces()
        {
            var contractId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90f7f";
            var command = new UpdateContractCommand("Expirado");

            var contract = Contract.CreateContract(
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f7f"),
                new ContractNumber(6235),
                new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new VehicleId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5t"),
                DateTime.UtcNow
            );

            _contractRepositoryMock.Setup(x => x.GetById(contractId)).ReturnsAsync(Optional<Contract>.Of(contract));
            _contractRepositoryMock.Setup(x => x.Update(contract)).ReturnsAsync(Result<Contract>.Success(contract));

            var result = await _handler.Execute((contractId, command));
            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public async Task ShouldFailToUpdateContractWhenContractNotFound()
        {
            var contractId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90f7f";
            var command = new UpdateContractCommand("Expirado");

            _contractRepositoryMock.Setup(x => x.GetById(contractId)).ReturnsAsync(Optional<Contract>.Empty());

            var result = await _handler.Execute((contractId, command));

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("Contract not found", result.ErrorMessage);
        }

        [Fact]
        public async Task ShouldFailToUpdateInsurancePolicyWhenUpdateFails()
        {
            var contractId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90f7f";
            var command = new UpdateContractCommand("Expirado");

            var contract = Contract.CreateContract(
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f7f"),
                new ContractNumber(6235),
                new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new VehicleId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5t"),
                DateTime.UtcNow
            );

            _contractRepositoryMock.Setup(x => x.GetById(contractId)).ReturnsAsync(Optional<Contract>.Of(contract));
            _contractRepositoryMock.Setup(x => x.Update(contract)).ReturnsAsync(Result<Contract>.Failure(new ContractUpdateFailedException()));

            var result = await _handler.Execute((contractId, command));

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("The contract could not be updated correctly", result.ErrorMessage);
        }
    }
}
