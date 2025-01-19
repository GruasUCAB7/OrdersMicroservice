using Moq;
using OrdersMS.Core.Utils.Optional;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Orders.Application.Queries.GetAllOrders;
using OrdersMS.src.Orders.Application.Queries.GetAllOrders.Types;
using OrdersMS.src.Orders.Application.Queries.GetExtraCostsByOrderId;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Domain;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;
using Xunit;

namespace OrdersMS.Tests.Orders.Application.Queries
{
    public class GetExtraCostsByOrderIdQueryHandlerTests
    {
        private readonly Mock<IExtraCostRepository> _extraCostRepositoryMock;
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly GetExtraCostsByOrderIdQueryHandler _handler;

        public GetExtraCostsByOrderIdQueryHandlerTests()
        {
            _extraCostRepositoryMock = new Mock<IExtraCostRepository>();
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _handler = new GetExtraCostsByOrderIdQueryHandler(_extraCostRepositoryMock.Object, _orderRepositoryMock.Object);
        }

        [Fact]
        public async Task GetExtraCostsByOrderIdSuccess()
        {
            var extraCosts = new List<ExtraCost>
            {
                new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d2r"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r"), new ExtraCostName("Cambio de Neumático"), new ExtraCostPrice(5)),
                new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d1w"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r"), new ExtraCostName("Desbloqueo del vehículo"), new ExtraCostPrice(2))            
            };

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
                new TotalCost(10),
                new OrderStatus("Por Aceptar")
            );

            _orderRepositoryMock.Setup(x => x.GetById(order.GetId())).ReturnsAsync(Optional<Order>.Of(order));
            _extraCostRepositoryMock.Setup(x => x.GetExtraCostByOrderId(order.GetId())).ReturnsAsync(extraCosts);
            
            var result = await _handler.Execute(order.GetId());

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            var response = result.Unwrap();
            Assert.Equal(2, response.ExtraCosts.Count);
            Assert.Equal("Cambio de Neumático", response.ExtraCosts[0].Name);
            Assert.Equal(5, response.ExtraCosts[0].Price);
            Assert.Equal("Desbloqueo del vehículo", response.ExtraCosts[1].Name);
            Assert.Equal(2, response.ExtraCosts[1].Price);
        }

        [Fact]
        public async Task GetExtraCostByOrderIdFailure_WhenNoOrderFound()
        {
            var extraCosts = new List<ExtraCost>
            {
                new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d2r"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r"), new ExtraCostName("Cambio de Neumático"), new ExtraCostPrice(5)),
                new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d1w"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r"), new ExtraCostName("Desbloqueo del vehículo"), new ExtraCostPrice(2))            };

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
                new TotalCost(10),
                new OrderStatus("Por Aceptar")
            );

            _orderRepositoryMock.Setup(x => x.GetById(order.GetId())).ReturnsAsync(Optional<Order>.Empty());

            var result = await _handler.Execute(order.GetId());

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("Order not found", result.ErrorMessage);
        }
    }
}
