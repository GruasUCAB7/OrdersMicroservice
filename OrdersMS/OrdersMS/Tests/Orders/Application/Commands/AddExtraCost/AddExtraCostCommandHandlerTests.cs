using Moq;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Utils.Optional;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Orders.Application.Commands.AddExtraCost;
using OrdersMS.src.Orders.Application.Commands.AddExtraCost.Types;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Queries.GetExtraCostsByOrderId.Types;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Domain;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;
using Xunit;

namespace OrdersMS.Tests.Orders.Application.Commands.AddExtraCost
{
    public class AddExtraCostCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IdGenerator<string>> _idGeneratorMock;
        private readonly AddExtraCostCommandHandler _handler;

        public AddExtraCostCommandHandlerTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _idGeneratorMock = new Mock<IdGenerator<string>>();
            _handler = new AddExtraCostCommandHandler(_orderRepositoryMock.Object);
        }

        [Fact]
        public async Task ShouldAddExtraCostToOrderSuccess()
        {
            var command = new AddExtraCostCommand(new List<ExtraCostDto> { new ExtraCostDto("53c0d8fa-dbca-4d98-9fdf-1d1413e90d5y", "Desbloqueo del vehículo", 100) });

            var order = Order.CreateOrder(
                new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new DriverId("Por asignar"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(0),
                new OrderStatus("Por Asignar")
            );

            _orderRepositoryMock.Setup(x => x.GetById(order.GetId())).ReturnsAsync(Optional<Order>.Of(order));
            _idGeneratorMock.Setup(x => x.Generate()).Returns("53c0d8fa-dbca-4d98-9fdf-1d1413e90d9w");
            _orderRepositoryMock.Setup(x => x.UpdateExtraCosts(It.IsAny<OrderId>(), It.IsAny<List<ExtraCost>>())).ReturnsAsync(Result<Order>.Success(order));

            var result = await _handler.Execute((order.GetId(), command));

            _orderRepositoryMock.Setup(x => x.Save(order)).ReturnsAsync(Result<Order>.Success(order));

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public async Task ShouldFailToAddExtraCost_WhenNoOrderFound()
        {
            var command = new AddExtraCostCommand(new List<ExtraCostDto> { new ExtraCostDto("53c0d8fa-dbca-4d98-9fdf-1d1413e90d5y", "Desbloqueo del vehículo", 100) });

            var order = Order.CreateOrder(
                new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new DriverId("Por asignar"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(0),
                new OrderStatus("Por Asignar")
            );

            _orderRepositoryMock.Setup(x => x.GetById(order.GetId())).ReturnsAsync(Optional<Order>.Empty());

            var result = await _handler.Execute((order.GetId(), command));

            Assert.Equal("Order not found", result.ErrorMessage);
        }

        [Fact]
        public async Task ShouldFail_WhenInvalidExtraCostName()
        {
            var command = new AddExtraCostCommand(new List<ExtraCostDto> { new ExtraCostDto("53c0d8fa-dbca-4d98-9fdf-1d1413e90d5y", "Desbloqueo del carro", 100) });

            var order = Order.CreateOrder(
                new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new DriverId("Por asignar"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(0),
                new OrderStatus("Por Asignar")
            );

            _orderRepositoryMock.Setup(x => x.GetById(order.GetId())).ReturnsAsync(Optional<Order>.Of(order));

            var result = await _handler.Execute((order.GetId(), command));

            Assert.True(result.IsFailure);
            Assert.Equal("Invalid extra cost name: Desbloqueo del carro", result.ErrorMessage);
        }

        [Fact]
        public async Task ShouldFailToUpdateExtraCostOrder_WhenUpdateFails()
        {
            var command = new AddExtraCostCommand(new List<ExtraCostDto> { new ExtraCostDto("53c0d8fa-dbca-4d98-9fdf-1d1413e90d5y", "Desbloqueo del vehículo", 100) });

            var order = Order.CreateOrder(
                new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new DriverId("Por asignar"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(0),
                new OrderStatus("Por Asignar")
            );

            _orderRepositoryMock.Setup(x => x.GetById(order.GetId())).ReturnsAsync(Optional<Order>.Of(order));
            _idGeneratorMock.Setup(x => x.Generate()).Returns("53c0d8fa-dbca-4d98-9fdf-1d1413e90d9w");
            _orderRepositoryMock.Setup(x => x.UpdateExtraCosts(It.IsAny<OrderId>(), It.IsAny<List<ExtraCost>>())).ReturnsAsync(Result<Order>.Failure(new ExtraServicesAppliedUpdateFailedException()));

            var result = await _handler.Execute((order.GetId(), command));

            _orderRepositoryMock.Setup(x => x.Save(order)).ReturnsAsync(Result<Order>.Success(order));

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("Extra services applied could not be updated correctly", result.ErrorMessage);
        }
    }
}