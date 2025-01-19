using Moq;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Utils.Optional;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Orders.Application.Commands.CreateExtraCost;
using OrdersMS.src.Orders.Application.Commands.CreateExtraCost.Types;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Domain;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;
using Xunit;

namespace OrdersMS.Tests.Orders.Application.Commands.CreateExtraCost
{
    public class CreateExtraCostCommandHandlerTests
    {
        private readonly Mock<IExtraCostRepository> _extraCostRepositoryMock;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IdGenerator<string>> _idGeneratorMock;
        private readonly CreateExtraCostCommandHandler _handler;

        public CreateExtraCostCommandHandlerTests()
        {
            _extraCostRepositoryMock = new Mock<IExtraCostRepository>();
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _idGeneratorMock = new Mock<IdGenerator<string>>();
            _handler = new CreateExtraCostCommandHandler(_extraCostRepositoryMock.Object, _orderRepositoryMock.Object, _idGeneratorMock.Object);
        }

        [Fact]
        public async Task ShouldCreateExtraCostSuccess()
        {
            var command = new CreateExtraCostCommand(
                "53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r",
                new List<ExtraCostDto2>() { }
            );

            var order = Order.CreateOrder(
                new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r"),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f6d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f9w"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5g"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>
                {
                    new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d2r"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r"), new ExtraCostName("Cambio de Neumático"), new ExtraCostPrice(5)),
                    new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d1w"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r"), new ExtraCostName("Desbloqueo del vehículo"), new ExtraCostPrice(2))
                },
                new TotalCost(80),
                new OrderStatus("Pagado")
            );

            _orderRepositoryMock.Setup(x => x.GetById(order.GetId())).ReturnsAsync(Optional<Order>.Of(order));
            _idGeneratorMock.Setup(x => x.Generate()).Returns("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a");

            var result = await _handler.Execute(command);

            var extraCost = new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d2r"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r"), new ExtraCostName("Cambio de Neumático"), new ExtraCostPrice(5));

            _extraCostRepositoryMock.Setup(x => x.Save(extraCost)).ReturnsAsync(Result<ExtraCost>.Success(extraCost));

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public async Task ShouldFailToCreateExtraCost_WhenNoOrderFound()
        {
            var command = new CreateExtraCostCommand(
                "53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r",
                new List<ExtraCostDto2>() { }
            );

            var order = Order.CreateOrder(
                new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r"),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f6d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f9w"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5g"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>
                {
                    new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d2r"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r"), new ExtraCostName("Cambio de Neumático"), new ExtraCostPrice(5)),
                    new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d1w"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r"), new ExtraCostName("Desbloqueo del vehículo"), new ExtraCostPrice(2))
                },
                new TotalCost(80),
                new OrderStatus("Pagado")
            );

            _orderRepositoryMock.Setup(x => x.GetById(order.GetId())).ReturnsAsync(Optional<Order>.Empty());

            var exception = await Assert.ThrowsAsync<OrderNotFoundException>(() => _handler.Execute(command));

            Assert.Equal("Order not found", exception.Message);
        }
    }
}