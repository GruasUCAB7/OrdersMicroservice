using MassTransit;
using Moq;
using OrdersMS.Core.Application.GoogleApiService;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Utils.Optional;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Exceptions;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Orders.Application.Commands.CreateOrder;
using OrdersMS.src.Orders.Application.Commands.CreateOrder.Types;
using OrdersMS.src.Orders.Application.Events;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Domain;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;
using Xunit;

namespace OrdersMS.Tests.Orders.Application.Commands.CreateOrder
{
    public class CreateOrderCommandHandlerTests
    {
        private readonly Mock<IOrderRepository> _orderRepositoryMock;
        private readonly Mock<IContractRepository> _contractRepositoryMock;
        private readonly Mock<IdGenerator<string>> _idGeneratorMock;
        private readonly Mock<IGoogleApiService> _googleServiceMock;
        private readonly Mock<IPublishEndpoint> _publishEndpointMock;
        private readonly CreateOrderCommandHandler _handler;

        public CreateOrderCommandHandlerTests()
        {
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _contractRepositoryMock = new Mock<IContractRepository>();
            _idGeneratorMock = new Mock<IdGenerator<string>>();
            _googleServiceMock = new Mock<IGoogleApiService>();
            _publishEndpointMock = new Mock<IPublishEndpoint>();
            _handler = new CreateOrderCommandHandler(_orderRepositoryMock.Object, _contractRepositoryMock.Object, _idGeneratorMock.Object, _googleServiceMock.Object, _publishEndpointMock.Object);
        }

        [Fact]
        public async Task ShouldCreateOrderSuccess()
        {
            var command = new CreateOrderCommand(
                "53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d",
                "53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a",
                "47c0d8fa-dbca-4d98-9fdf-1d1413e90f00",
                "El paraiso, caracas",
                "Altamira, caracas",
                "Fallo de Frenos",
                new List<string>() { }
            );

            var contract = Contract.CreateContract(
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new ContractNumber(6235),
                new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f80"),
                new VehicleId("47c0d8fa-dbca-4d98-9fdf-1d1413e90f00"),
                DateTime.UtcNow
            );

            _contractRepositoryMock.Setup(x => x.GetById(command.ContractId)).ReturnsAsync(Optional<Contract>.Of(contract));
            _contractRepositoryMock.Setup(x => x.IsActiveContract(command.ContractId)).ReturnsAsync(true);
            _googleServiceMock.Setup(x => x.GetCoordinatesFromAddress(command.IncidentAddress)).ReturnsAsync(Result<Core.Infrastructure.GoogleMaps.Coordinates>.Success(new Core.Infrastructure.GoogleMaps.Coordinates { Latitude = 10.0, Longitude = 10.0 }));
            _googleServiceMock.Setup(x => x.GetCoordinatesFromAddress(command.DestinationAddress)).ReturnsAsync(Result<Core.Infrastructure.GoogleMaps.Coordinates>.Success(new Core.Infrastructure.GoogleMaps.Coordinates { Latitude = 10.0, Longitude = 10.0 }));
            _idGeneratorMock.Setup(x => x.Generate()).Returns("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a");
            _publishEndpointMock.Setup(x => x.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _handler.Execute(command);

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

            _orderRepositoryMock.Setup(x => x.Save(order)).ReturnsAsync(Result<Order>.Success(order));

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public async Task ShouldFailToCreateOrder_WhenNoContractFound()
        {
            var command = new CreateOrderCommand(
                "53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d",
                "53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a",
                "47c0d8fa-dbca-4d98-9fdf-1d1413e90f00",
                "El paraiso, caracas",
                "Altamira, caracas",
                "Fallo de Frenos",
                new List<string>() { }
            );

            _contractRepositoryMock.Setup(x => x.GetById(command.ContractId)).ReturnsAsync(Optional<Contract>.Empty());

            var exception = await Assert.ThrowsAsync<ContractNotFoundException>(() => _handler.Execute(command));

            Assert.Equal("Contract not found", exception.Message);
        }

        [Fact]
        public async Task ShouldFailToCreateOrder_WhenContractIsNotActive()
        {
            var command = new CreateOrderCommand(
                "53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d",
                "53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a",
                "47c0d8fa-dbca-4d98-9fdf-1d1413e90f00",
                "El paraiso, caracas",
                "Altamira, caracas",
                "Fallo de Frenos",
                new List<string>() { }
            );

            var contract = Contract.CreateContract(
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new ContractNumber(6235),
                new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f80"),
                new VehicleId("47c0d8fa-dbca-4d98-9fdf-1d1413e90f00"),
                DateTime.UtcNow
            );

            _contractRepositoryMock.Setup(x => x.GetById(command.ContractId)).ReturnsAsync(Optional<Contract>.Of(contract));
            _contractRepositoryMock.Setup(x => x.IsActiveContract(command.ContractId)).ReturnsAsync(false);

            var exception = await Assert.ThrowsAsync<ContractNotAvailableException>(() => _handler.Execute(command));

            Assert.Equal("Contract is not active", exception.Message);
        }
    }
}