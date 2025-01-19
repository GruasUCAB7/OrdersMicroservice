﻿using MassTransit;
using Moq;
using OrdersMS.Core.Utils.Optional;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatusToCompleted;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatusToCompleted.Types;
using OrdersMS.src.Orders.Application.Events;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Domain;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;
using Xunit;

namespace OrdersMS.Tests.Orders.Application.Commands.UpdateOrderStatusToCompleted
{
    public class UpdateOrderStatusToPaidCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly UpdateOrderStatusToCompletedCommandHandler _handler;

        public UpdateOrderStatusToPaidCommandHandlerTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _handler = new UpdateOrderStatusToCompletedCommandHandler(_orderRepositoryMock.Object, _publishEndpointMock.Object);
        }

        [Fact]
        public async Task ShouldUpdateOrderStatusToCompletedSuccess()
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
            var command = new UpdateOrderStatusToCompletedCommand(true);

            _orderRepositoryMock.Setup(x => x.GetById(order.GetId())).ReturnsAsync(Optional<Order>.Of(order));
            _orderRepositoryMock.Setup(x => x.Update(order)).ReturnsAsync(Result<Order>.Success(order));
            _publishEndpointMock.Setup(x => x.Publish(It.IsAny<OrderCompletedEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _handler.Execute((order.GetId(), command));

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            _orderRepositoryMock.Verify(x => x.Update(order), Times.Once);
            _publishEndpointMock.Verify(x => x.Publish(It.IsAny<OrderCompletedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
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
            var command = new UpdateOrderStatusToCompletedCommand(true);

            _orderRepositoryMock.Setup(x => x.GetById(order.GetId())).ReturnsAsync(Optional<Order>.Empty());

            var result = await _handler.Execute((order.GetId(), command));

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("Order not found", result.ErrorMessage);
        }

        [Fact]
        public async Task ShouldFailToUpdateOrderWhenUpdateFails()
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
            var command = new UpdateOrderStatusToCompletedCommand(true);

            _orderRepositoryMock.Setup(x => x.GetById(order.GetId())).ReturnsAsync(Optional<Order>.Of(order));
            _orderRepositoryMock.Setup(x => x.Update(order)).ReturnsAsync(Result<Order>.Failure(new OrderUpdateFailedException("Order status could not be updated correctly")));

            var result = await _handler.Execute((order.GetId(), command));

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("Order status could not be updated correctly", result.ErrorMessage);
        }

        [Fact]
        public async Task ShouldFailToUpdateOrderWhenOrderStatusIsDifferentToInProgress()
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
                new OrderStatus(OrderStatus.Aceptado)
            );
            var command = new UpdateOrderStatusToCompletedCommand(true);

            _orderRepositoryMock.Setup(x => x.GetById(order.GetId())).ReturnsAsync(Optional<Order>.Of(order));
            _orderRepositoryMock.Setup(x => x.Update(order)).ReturnsAsync(Result<Order>.Failure(new OrderUpdateFailedException("The order is not in the En Proceso status")));

            var result = await _handler.Execute((order.GetId(), command));

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("The order is not in the En Proceso status", result.ErrorMessage);
        }
    }
}