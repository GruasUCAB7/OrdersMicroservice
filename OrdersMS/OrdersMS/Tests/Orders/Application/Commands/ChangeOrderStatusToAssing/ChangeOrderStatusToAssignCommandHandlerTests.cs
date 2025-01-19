using Moq;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Orders.Application.Commands.ChangeOrderStatusToAssing;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Domain;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;
using Xunit;

namespace OrdersMS.Tests.Orders.Application.Commands.ChangeOrderStatusToAssing
{
    public class ChangeOrderStatusToAssignCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly ChangeOrderStatusToAssignCommandHandler _handler;

        public ChangeOrderStatusToAssignCommandHandlerTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _handler = new ChangeOrderStatusToAssignCommandHandler(_orderRepositoryMock.Object);
        }

        [Fact]
        public async Task ShouldChangeOrderStatusToAssignSuccess()
        {
            var originalOrders = new List<Order> {
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
                    new TotalCost(0),
                    new OrderStatus("Por Aceptar")
                ),
                Order.CreateOrder(
                    new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d4l"),
                    new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f3e"),
                    new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f9r"),
                    new DriverId("Por asignar"),
                    new Coordinates(10.0, 10.0),
                    new Coordinates(11.0, 11.0),
                    new IncidentType("Fallo de Frenos"),
                    DateTime.UtcNow,
                    new List<ExtraCost>(),
                    new TotalCost(0),
                    new OrderStatus("Por Aceptar")
                ),
            };

            _orderRepositoryMock.Setup(x => x.ValidateUpdateTimeForStatusPorAceptar()).ReturnsAsync(originalOrders);
            _orderRepositoryMock.Setup(x => x.Update(It.IsAny<Order>())).ReturnsAsync(Result<Order>.Success(originalOrders.First()));

            var result = await _handler.Execute();

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.Equal(2, result.Unwrap().Count);
            _orderRepositoryMock.Verify(x => x.Update(It.IsAny<Order>()), Times.Exactly(2));
        }

        [Fact]
        public async Task ShouldReturnFailure_WhenUpdateFails()
        {
            var originalOrders = new List<Order> {
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
                    new TotalCost(0),
                    new OrderStatus("Por Aceptar")
                )
            };

            _orderRepositoryMock.Setup(x => x.ValidateUpdateTimeForStatusPorAceptar()).ReturnsAsync(originalOrders);
            _orderRepositoryMock.Setup(x => x.Update(It.IsAny<Order>())).ReturnsAsync(Result<Order>.Failure(new OrderUpdateFailedException("Order could not be updated correctly")));

            var result = await _handler.Execute();

            Assert.NotNull(result);
            Assert.True(result.IsFailure);
            Assert.Equal("Order could not be updated correctly", result.ErrorMessage);
        }
    }
}