using Moq;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Orders.Application.Queries.GetAllOrdersByDriverAssigned;
using OrdersMS.src.Orders.Application.Queries.GetAllOrdersByDriverAssigned.Types;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Domain;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;
using Xunit;

namespace OrdersMS.Tests.Orders.Application.Queries
{
    public class GetAllOrdersByDriverAssignedQueryHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly GetAllOrdersByDriverAssignedQueryHandler _handler;

        public GetAllOrdersByDriverAssignedQueryHandlerTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _handler = new GetAllOrdersByDriverAssignedQueryHandler(_orderRepositoryMock.Object);
        }

        [Fact]
        public async Task GetAllOrdersByDriverAssignedSuccess()
        {
            var driverId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90f5g";
            var query = new GetAllOrdersByDriverAssignedQuery(5, 1);
            var orders = new List<Order>
            {
                Order.CreateOrder(
                    new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"),
                    new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                    new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                    new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5g"),
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

            _orderRepositoryMock.Setup(x => x.GetAllOrdersByDriverAssigned(query, driverId)).ReturnsAsync(orders);
            var result = await _handler.Execute((query, driverId));

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.Equal(2, result.Unwrap().Length);
        }

        [Fact]
        public async Task GetAllOrdersByDriverAssignedFailureWhenNoOrdersFound()
        {
            var driverId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90f5g";
            var query = new GetAllOrdersByDriverAssignedQuery(5, 1);

            _orderRepositoryMock.Setup(x => x.GetAllOrdersByDriverAssigned(query, driverId)).ReturnsAsync(new List<Order>());

            var result = await _handler.Execute((query, driverId));

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.Empty(result.Unwrap());
        }
    }
}
