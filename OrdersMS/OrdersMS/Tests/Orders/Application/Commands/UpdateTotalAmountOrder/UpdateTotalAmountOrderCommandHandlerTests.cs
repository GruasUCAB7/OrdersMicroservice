using Moq;
using OrdersMS.Core.Utils.Optional;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain;
using OrdersMS.src.Contracts.Domain.Entities;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Orders.Application.Commands.UpdateTotalAmountOrder;
using OrdersMS.src.Orders.Application.Commands.UpdateTotalAmountOrder.Types;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Domain;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.Services;
using OrdersMS.src.Orders.Domain.Services.Types;
using OrdersMS.src.Orders.Domain.ValueObjects;
using Xunit;

namespace OrdersMS.Tests.Orders.Application.Commands.UpdateTotalAmountOrder
{
    public class UpdateTotalAmountOrderCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IContractRepository> _contractRepositoryMock;
        private readonly Mock<IPolicyRepository> _policyRepositoryMock;
        private readonly Mock<CalculateOrderTotalAmount> _calculateTotalAmountMock;
        private readonly UpdateTotalAmountOrderCommandHandler _handler;

        public UpdateTotalAmountOrderCommandHandlerTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _contractRepositoryMock = new Mock<IContractRepository>();
            _policyRepositoryMock = new Mock<IPolicyRepository>();
            _calculateTotalAmountMock = new Mock<CalculateOrderTotalAmount>();
            _handler = new UpdateTotalAmountOrderCommandHandler(_orderRepositoryMock.Object, _contractRepositoryMock.Object, _policyRepositoryMock.Object, _calculateTotalAmountMock.Object);
        }

        [Fact]
        public async Task ShouldUpdateTotalAmountOrderSuccess()
        {
            var order = Order.CreateOrder(
                new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f54"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>() { },
                new TotalCost(0),
                new OrderStatus(OrderStatus.EnProceso)
            );

            var contract = Contract.CreateContract(
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new ContractNumber(6235),
                new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f80"),
                new VehicleId("47c0d8fa-dbca-4d98-9fdf-1d1413e90f00"),
                DateTime.UtcNow
            );

            var policy = new InsurancePolicy(
                new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f80"),
                new PolicyType("Diamante"),
                new PolicyCoverageKm(25),
                new PolicyIncidentCoverageAmount(50),
                new PriceExtraKm(3)
            );

            var totalCost = new TotalCost(500);

            var command = new UpdateTotalAmountOrderCommand(25);

            _orderRepositoryMock.Setup(x => x.GetById(order.GetId())).ReturnsAsync(Optional<Order>.Of(order));
            _contractRepositoryMock.Setup(x => x.GetById(contract.GetId())).ReturnsAsync(Optional<Contract>.Of(contract));
            _policyRepositoryMock.Setup(x => x.GetById(policy.GetId())).ReturnsAsync(Optional<InsurancePolicy>.Of(policy));
            _calculateTotalAmountMock.Setup(x => x.Execute(It.IsAny<CalculateOrderTotalAmountInput>())).Returns(totalCost);
            _orderRepositoryMock.Setup(x => x.Update(order)).ReturnsAsync(Result<Order>.Success(order));

            var result = await _handler.Execute((order.GetId(), command));

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.Equal(totalCost.GetValue(), order.GetTotalCost());
            _orderRepositoryMock.Verify(x => x.Update(order), Times.Once);
        }

        [Fact]
        public async Task ShouldFailToUpdateOrderWhenOrderNotFound()
        {
            var order = Order.CreateOrder(
                new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f54"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(0),
                new OrderStatus(OrderStatus.EnProceso)
            );
            var command = new UpdateTotalAmountOrderCommand(25);

            _orderRepositoryMock.Setup(x => x.GetById(order.GetId())).ReturnsAsync(Optional<Order>.Empty());

            var result = await _handler.Execute((order.GetId(), command));

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("Order not found", result.ErrorMessage);
        }

        [Fact]
        public async Task ShouldFailToUpdateOrderWhenContractNotFound()
        {
            var order = Order.CreateOrder(
                new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f54"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(0),
                new OrderStatus(OrderStatus.EnProceso)
            );
            var command = new UpdateTotalAmountOrderCommand(25);

            _orderRepositoryMock.Setup(x => x.GetById(order.GetId())).ReturnsAsync(Optional<Order>.Of(order));
            _contractRepositoryMock.Setup(x => x.GetById(order.GetContractId())).ReturnsAsync(Optional<Contract>.Empty());

            var result = await _handler.Execute((order.GetId(), command));

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("Contract not found", result.ErrorMessage);
        }

        [Fact]
        public async Task ShouldFailToUpdateOrderWhenPolicyNotFound()
        {
            var order = Order.CreateOrder(
                new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f54"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(0),
                new OrderStatus(OrderStatus.EnProceso)
            );

            var contract = Contract.CreateContract(
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new ContractNumber(6235),
                new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f80"),
                new VehicleId("47c0d8fa-dbca-4d98-9fdf-1d1413e90f00"),
                DateTime.UtcNow
            );

            var command = new UpdateTotalAmountOrderCommand(25);

            _orderRepositoryMock.Setup(x => x.GetById(order.GetId())).ReturnsAsync(Optional<Order>.Of(order));
            _contractRepositoryMock.Setup(x => x.GetById(order.GetContractId())).ReturnsAsync(Optional<Contract>.Of(contract));
            _policyRepositoryMock.Setup(x => x.GetById(contract.GetPolicyId())).ReturnsAsync(Optional<InsurancePolicy>.Empty());

            var result = await _handler.Execute((order.GetId(), command));

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("Policy not found", result.ErrorMessage);
        }

        [Fact]
        public async Task ShouldFailToUpdateOrderTotalAmountWhenUpdateFails()
        {
            var order = Order.CreateOrder(
                new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f54"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>() { },
                new TotalCost(0),
                new OrderStatus(OrderStatus.EnProceso)
            );

            var contract = Contract.CreateContract(
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new ContractNumber(6235),
                new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f80"),
                new VehicleId("47c0d8fa-dbca-4d98-9fdf-1d1413e90f00"),
                DateTime.UtcNow
            );

            var policy = new InsurancePolicy(
                new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f80"),
                new PolicyType("Diamante"),
                new PolicyCoverageKm(25),
                new PolicyIncidentCoverageAmount(50),
                new PriceExtraKm(3)
            );

            var totalCost = new TotalCost(500);

            var command = new UpdateTotalAmountOrderCommand(25);

            _orderRepositoryMock.Setup(x => x.GetById(order.GetId())).ReturnsAsync(Optional<Order>.Of(order));
            _contractRepositoryMock.Setup(x => x.GetById(contract.GetId())).ReturnsAsync(Optional<Contract>.Of(contract));
            _policyRepositoryMock.Setup(x => x.GetById(policy.GetId())).ReturnsAsync(Optional<InsurancePolicy>.Of(policy));
            _calculateTotalAmountMock.Setup(x => x.Execute(It.IsAny<CalculateOrderTotalAmountInput>())).Returns(totalCost);
            _orderRepositoryMock.Setup(x => x.Update(order)).ReturnsAsync(Result<Order>.Failure(new OrderUpdateFailedException("Order total amount could not be updated correctly")));

            var result = await _handler.Execute((order.GetId(), command));

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("Order total amount could not be updated correctly", result.ErrorMessage);
        }
    }
}