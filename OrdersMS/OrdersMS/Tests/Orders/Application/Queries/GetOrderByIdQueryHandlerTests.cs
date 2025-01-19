using Moq;
using OrdersMS.Core.Utils.Optional;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Orders.Application.Queries.GetOrderById;
using OrdersMS.src.Orders.Application.Queries.GetOrderById.Types;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Domain;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;
using Xunit;

namespace OrdersMS.Tests.Orders.Application.Queries
{
    public class GetOrderByIdQueryHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly GetOrderByIdQueryHandler _handler;

        public GetOrderByIdQueryHandlerTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _handler = new GetOrderByIdQueryHandler(_orderRepositoryMock.Object);
        }

        [Fact]
        public async Task GetOrderByIdSuccess()
        {
            var query = new GetOrderByIdQuery("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");
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

            _orderRepositoryMock.Setup(x => x.GetById(query.Id)).ReturnsAsync(Optional<Order>.Of(order));

            var result = await _handler.Execute(query);

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public async Task GetOrderByIdWhenOrderNotFound()
        {
            var query = new GetOrderByIdQuery("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");

            _orderRepositoryMock.Setup(x => x.GetById(query.Id)).ReturnsAsync(Optional<Order>.Empty());

            var result = await _handler.Execute(query);

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("Order not found", result.ErrorMessage);
        }
    }
}
