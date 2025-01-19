using Moq;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Orders.Application.Queries.GetAllOrders;
using OrdersMS.src.Orders.Application.Queries.GetAllOrders.Types;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Domain;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;
using Xunit;

namespace OrdersMS.Tests.Orders.Application.Queries
{
    public class GetAllOrdersQueryHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly GetAllOrdersQueryHandler _handler;

        public GetAllOrdersQueryHandlerTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _handler = new GetAllOrdersQueryHandler(_orderRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllOrdersSuccess()
        {
            var query = new GetAllOrdersQuery(5, 1, "");
            var orders = new List<Order>
            {
                Order.CreateOrder(
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
                ),
                Order.CreateOrder(
                    new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r"),
                    new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f6d"),
                    new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f9w"),
                    new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5g"),
                    new Coordinates(10.0, 10.0),
                    new Coordinates(11.0, 11.0),
                    new IncidentType("Fallo de Frenos"),
                    DateTime.UtcNow,
                    new List<ExtraCost>(),
                    new TotalCost(80),
                    new OrderStatus("Pagado")
                )
            };

            _orderRepositoryMock.Setup(x => x.GetAll(query)).ReturnsAsync(orders);
            var result = await _handler.Execute(query);

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.Equal(2, result.Unwrap().Length);
        }

        [Fact]
        public async Task GetAllOrdersFailureWhenNoOrdersFound()
        {
            var query = new GetAllOrdersQuery(1, 5, "");

            _orderRepositoryMock.Setup(x => x.GetAll(query)).ReturnsAsync(new List<Order>());

            var result = await _handler.Execute(query);

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.Empty(result.Unwrap());
        }
    }
}
