using Moq;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Utils.Optional;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Orders.Application.Commands.AddExtraCost;
using OrdersMS.src.Orders.Application.Commands.AddExtraCost.Types;
using OrdersMS.src.Orders.Application.Commands.ValidatePricesOfExtrasCost;
using OrdersMS.src.Orders.Application.Commands.ValidatePricesOfExtrasCost.Types;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Queries.GetExtraCostsByOrderId.Types;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.Types;
using OrdersMS.src.Orders.Domain;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;
using Xunit;

namespace OrdersMS.Tests.Orders.Application.Commands.ValidatePricesOfExtrasCost
{
    public class ValidatePricesOfExtrasCostCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<AddExtraCostCommandHandler> _addExtraCostMock;
        private readonly ValidatePricesOfExtrasCostCommandHandler _handler;

        public ValidatePricesOfExtrasCostCommandHandlerTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _addExtraCostMock = new Mock<AddExtraCostCommandHandler>(_orderRepositoryMock.Object);
            _handler = new ValidatePricesOfExtrasCostCommandHandler(_orderRepositoryMock.Object, _addExtraCostMock.Object);
        }

        [Fact]
        public async Task ShouldValidatePricesOfExtraCostSuccess()
        {
            var extraCosts = new List<ExtraCost>
            {
                new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d2r"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"), new ExtraCostName("Cambio de Neumático"), new ExtraCostPrice(5)),
                new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d1w"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"), new ExtraCostName("Desbloqueo del vehículo"), new ExtraCostPrice(2))
            };

            var extraCostsDto = new List<ExtraCostDto>
            {
                new ExtraCostDto("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a", "Cambio de Neumático", 5),
                new ExtraCostDto("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a", "Desbloqueo del vehículo", 2)
            };

            var order = Order.CreateOrder(
                new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f54"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                extraCosts,
                new TotalCost(0),
                new OrderStatus(OrderStatus.Aceptado)
            );
            var command = new ValidatePricesOfExtrasCostCommand(true, extraCostsDto);

            var extraServices = order.GetExtrasServicesApplied().Select(extraCost => new ExtraServiceDto(
                extraCost.GetId(),
                extraCost.GetName(),
                extraCost.GetPrice()
            )).ToList();

            var addExtraCostResult = Result<GetOrderResponse>.Success(new GetOrderResponse(
                order.GetId(),
                order.GetContractId(),
                order.GetOperatorAssigned(),
                order.GetDriverAssigned(),
                new CoordinatesDto(order.GetIncidentAddressLatitude(), order.GetIncidentAddressLongitude()),
                new CoordinatesDto(order.GetDestinationAddressLatitude(), order.GetDestinationAddressLongitude()),
                order.GetIncidentType(),
                order.GetIncidentDate(),
                extraServices,
                order.GetTotalCost(),
                order.GetOrderStatus())
            );

            _orderRepositoryMock.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(Optional<Order>.Of(order));
            _addExtraCostMock.Setup(x => x.Execute(It.IsAny<(string, AddExtraCostCommand)>())).ReturnsAsync(addExtraCostResult);
            _orderRepositoryMock.Setup(x => x.Update(It.IsAny<Order>())).ReturnsAsync(Result<Order>.Success(order));

            var result = await _handler.Execute((order.GetId(), command));

            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public async Task ShouldFailToValidatePricesOfExtraCost_WhenOrderNotFound()
        {
            var extraCosts = new List<ExtraCost>
            {
                new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d2r"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"), new ExtraCostName("Cambio de Neumático"), new ExtraCostPrice(5)),
                new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d1w"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"), new ExtraCostName("Desbloqueo del vehículo"), new ExtraCostPrice(2))
            };

            var extraCostsDto = new List<ExtraCostDto>
            {
                new ExtraCostDto("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a", "Cambio de Neumático", 5),
                new ExtraCostDto("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a", "Desbloqueo del vehículo", 2)
            };

            var order = Order.CreateOrder(
                new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f54"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                extraCosts,
                new TotalCost(0),
                new OrderStatus(OrderStatus.Aceptado)
            );
            var command = new ValidatePricesOfExtrasCostCommand(true, extraCostsDto);

            _orderRepositoryMock.Setup(x => x.GetById(order.GetId())).ReturnsAsync(Optional<Order>.Empty());

            var result = await _handler.Execute((order.GetId(), command));

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("Order not found", result.ErrorMessage);
        }

        [Fact]
        public async Task ShouldFailToValidatePricesOfExtraCost_WhenAddExtraCostFailed()
        {
            var extraCosts = new List<ExtraCost>
            {
                new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d2r"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"), new ExtraCostName("Cambio de Neumático"), new ExtraCostPrice(5)),
                new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d1w"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"), new ExtraCostName("Desbloqueo del vehículo"), new ExtraCostPrice(2))
            };

            var extraCostsDto = new List<ExtraCostDto>
            {
                new ExtraCostDto("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a", "Cambio de Neumático", 5),
                new ExtraCostDto("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a", "Desbloqueo del vehículo", 2)
            };

            var order = Order.CreateOrder(
                new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f54"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                extraCosts,
                new TotalCost(0),
                new OrderStatus(OrderStatus.Aceptado)
            );
            var command = new ValidatePricesOfExtrasCostCommand(true, extraCostsDto);

            var extraServices = order.GetExtrasServicesApplied().Select(extraCost => new ExtraServiceDto(
                extraCost.GetId(),
                extraCost.GetName(),
                extraCost.GetPrice()
            )).ToList();

            var addExtraCostResult = Result<GetOrderResponse>.Success(new GetOrderResponse(
                order.GetId(),
                order.GetContractId(),
                order.GetOperatorAssigned(),
                order.GetDriverAssigned(),
                new CoordinatesDto(order.GetIncidentAddressLatitude(), order.GetIncidentAddressLongitude()),
                new CoordinatesDto(order.GetDestinationAddressLatitude(), order.GetDestinationAddressLongitude()),
                order.GetIncidentType(),
                order.GetIncidentDate(),
                extraServices,
                order.GetTotalCost(),
                order.GetOrderStatus()));

            _orderRepositoryMock.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(Optional<Order>.Of(order));
            _addExtraCostMock.Setup(x => x.Execute(It.IsAny<(string, AddExtraCostCommand)>())).ReturnsAsync(Result<GetOrderResponse>.Failure(new FailedToAddingExtraCostExtraCost()));
            _orderRepositoryMock.Setup(x => x.Update(It.IsAny<Order>())).ReturnsAsync(Result<Order>.Success(order));

            var result = await _handler.Execute((order.GetId(), command));

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("There was an error adding the extra cost supplied.", result.ErrorMessage);
        }

        [Fact]
        public async Task ShouldFailToValidatePricesOfExtraCost_WhenUpdateFails()
        {
            var extraCosts = new List<ExtraCost>
            {
                new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d2r"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"), new ExtraCostName("Cambio de Neumático"), new ExtraCostPrice(5)),
                new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d1w"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"), new ExtraCostName("Desbloqueo del vehículo"), new ExtraCostPrice(2))
            };

            var extraCostsDto = new List<ExtraCostDto>
            {
                new ExtraCostDto("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a", "Cambio de Neumático", 5),
                new ExtraCostDto("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a", "Desbloqueo del vehículo", 2)
            };

            var order = Order.CreateOrder(
                new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f54"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                extraCosts,
                new TotalCost(0),
                new OrderStatus(OrderStatus.Aceptado)
            );
            var command = new ValidatePricesOfExtrasCostCommand(true, extraCostsDto);

            var extraServices = order.GetExtrasServicesApplied().Select(extraCost => new ExtraServiceDto(
                extraCost.GetId(),
                extraCost.GetName(),
                extraCost.GetPrice()
            )).ToList();

            var addExtraCostResult = Result<GetOrderResponse>.Success(new GetOrderResponse(
                order.GetId(),
                order.GetContractId(),
                order.GetOperatorAssigned(),
                order.GetDriverAssigned(),
                new CoordinatesDto(order.GetIncidentAddressLatitude(), order.GetIncidentAddressLongitude()),
                new CoordinatesDto(order.GetDestinationAddressLatitude(), order.GetDestinationAddressLongitude()),
                order.GetIncidentType(),
                order.GetIncidentDate(),
                extraServices,
                order.GetTotalCost(),
                order.GetOrderStatus()));

            _orderRepositoryMock.Setup(x => x.GetById(It.IsAny<string>())).ReturnsAsync(Optional<Order>.Of(order));
            _addExtraCostMock.Setup(x => x.Execute(It.IsAny<(string, AddExtraCostCommand)>())).ReturnsAsync(addExtraCostResult);
            _orderRepositoryMock.Setup(x => x.Update(It.IsAny<Order>())).ReturnsAsync(Result<Order>.Failure(new OrderUpdateFailedException("The order could not be updated correctly")));

            var result = await _handler.Execute((order.GetId(), command));

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("The order could not be updated correctly", result.ErrorMessage);
        }
    }
}