using FluentValidation;
using FluentValidation.Results;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrdersMS.Core.Application.Firebase;
using OrdersMS.Core.Application.GoogleApiService;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Application.Logger;
using OrdersMS.Core.Utils.Optional;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain;
using OrdersMS.src.Contracts.Domain.Entities;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Orders.Application.Commands.AddExtraCost;
using OrdersMS.src.Orders.Application.Commands.AddExtraCost.Types;
using OrdersMS.src.Orders.Application.Commands.CreateExtraCost.Types;
using OrdersMS.src.Orders.Application.Commands.CreateOrder.Types;
using OrdersMS.src.Orders.Application.Commands.UpdateDriverAssigned.Types;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatus.Types;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatusToCompleted.Types;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatusToPaid.Types;
using OrdersMS.src.Orders.Application.Commands.UpdateTotalAmountOrder.Types;
using OrdersMS.src.Orders.Application.Commands.ValidateLocationDriverToIncidecident.Types;
using OrdersMS.src.Orders.Application.Commands.ValidatePricesOfExtrasCost.Types;
using OrdersMS.src.Orders.Application.Events;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Queries.GetAllOrders.Types;
using OrdersMS.src.Orders.Application.Queries.GetAllOrdersByDriverAssigned.Types;
using OrdersMS.src.Orders.Application.Queries.GetExtraCostsByOrderId.Types;
using OrdersMS.src.Orders.Application.Queries.GetOrderById.Types;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.Types;
using OrdersMS.src.Orders.Domain;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.Services;
using OrdersMS.src.Orders.Domain.Services.Types;
using OrdersMS.src.Orders.Domain.ValueObjects;
using OrdersMS.src.Orders.Infrastructure.Controller;
using RestSharp;
using Xunit;

namespace OrdersMS.Tests.Orders.Infrastructure
{
    public class OrderControllerTests
    {
        private readonly Mock<IOrderRepository> _orderRepoMock = new Mock<IOrderRepository>();
        private readonly Mock<IContractRepository> _contractRepoMock = new Mock<IContractRepository>();
        private readonly Mock<IPolicyRepository> _policyRepoMock = new Mock<IPolicyRepository>();
        private readonly Mock<IExtraCostRepository> _extraCostRepoMock = new Mock<IExtraCostRepository>();
        private readonly Mock<IdGenerator<string>> _idGeneratorMock = new Mock<IdGenerator<string>>();
        private readonly Mock<IValidator<CreateOrderCommand>> _validatorCreateMock = new Mock<IValidator<CreateOrderCommand>>();
        private readonly Mock<IValidator<ValidatePricesOfExtrasCostCommand>> _validatorPricesExtrasCostMock = new Mock<IValidator<ValidatePricesOfExtrasCostCommand>>();
        private readonly Mock<IValidator<UpdateDriverAssignedCommand>> _validatorDriverAssignedMock = new Mock<IValidator<UpdateDriverAssignedCommand>>();
        private readonly Mock<IValidator<UpdateOrderStatusCommand>> _validatorOrderStatusMock = new Mock<IValidator<UpdateOrderStatusCommand>>();
        private readonly Mock<IValidator<ValidateLocationCommand>> _validatorLocationDriverMock = new Mock<IValidator<ValidateLocationCommand>>();
        private readonly Mock<IValidator<UpdateTotalAmountOrderCommand>> _validatorTotalAmountMock = new Mock<IValidator<UpdateTotalAmountOrderCommand>>();
        private readonly Mock<IValidator<UpdateOrderStatusToCompletedCommand>> _validatorOrderstatusToCompletedMock = new Mock<IValidator<UpdateOrderStatusToCompletedCommand>>();
        private readonly Mock<IValidator<UpdateOrderStatusToPaidCommand>> _validatorOrderstatusToPaidMock = new Mock<IValidator<UpdateOrderStatusToPaidCommand>>();
        private readonly Mock<IValidator<CreateExtraCostCommand>> _validatorCreateExtraCostMock = new Mock<IValidator<CreateExtraCostCommand>>();
        private readonly Mock<CalculateOrderTotalAmount> _calculateOrderTotalAmountMock = new Mock<CalculateOrderTotalAmount>();
        private readonly Mock<AddExtraCostCommandHandler> _addExtraCostMock;
        private readonly Mock<IGoogleApiService> _googleApiServiceMock = new Mock<IGoogleApiService>();
        private readonly Mock<IPublishEndpoint> _publishEndpointMock = new Mock<IPublishEndpoint>();
        private readonly Mock<IRestClient> _restClientMock = new Mock<IRestClient>();
        private readonly Mock<IBus> _busMock = new Mock<IBus>();
        private readonly Mock<IFirebaseMessagingService> _firebaseServiceMock = new Mock<IFirebaseMessagingService>();
        private readonly Mock<ILoggerContract> _loggerMock = new Mock<ILoggerContract>();
        private readonly OrderController _controller;

        public OrderControllerTests()
        {
            _addExtraCostMock = new Mock<AddExtraCostCommandHandler>(_orderRepoMock.Object);
            _controller = new OrderController(
                _orderRepoMock.Object,
                _contractRepoMock.Object,
                _policyRepoMock.Object,
                _extraCostRepoMock.Object,
                _idGeneratorMock.Object,
                _validatorCreateMock.Object,
                _validatorPricesExtrasCostMock.Object,
                _validatorDriverAssignedMock.Object,
                _validatorOrderStatusMock.Object,
                _validatorLocationDriverMock.Object,
                _validatorTotalAmountMock.Object,
                _validatorOrderstatusToCompletedMock.Object,
                _validatorOrderstatusToPaidMock.Object,
                _validatorCreateExtraCostMock.Object,
                _calculateOrderTotalAmountMock.Object,
                _addExtraCostMock.Object,
                _googleApiServiceMock.Object,
                _publishEndpointMock.Object,
                _restClientMock.Object,
                _busMock.Object,
                _firebaseServiceMock.Object,
                _loggerMock.Object
            );
        }

        [Fact]
        public async Task CreateOrder_ShouldReturnCreated()
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

            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeIeyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeI";
            var restResponse = new RestResponse
            {
                Content = "{\"id\":\"53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a\",\"name\":\"Operator\",\"email\":\"operator@gmail.com\",\"phone\":\"+58 424-2720504\",\"userType\":\"Operator\",\"isActive\":true,\"department\":\"Service\",\"isTemporaryPassword\":false,\"passwordExpirationDate\":\"2125-11-01T23:25:59\"}",
                StatusCode = System.Net.HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed,
                ContentType = "application/json",
                IsSuccessStatusCode = true,
                ResponseUri = new Uri("https://localhost:4051/user/53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                StatusDescription = "OK"
            };

            _restClientMock.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(restResponse);
            _validatorCreateMock.Setup(x => x.Validate(command)).Returns(new FluentValidation.Results.ValidationResult());
            _contractRepoMock.Setup(r => r.GetById(contract.GetId())).ReturnsAsync(Optional<Contract>.Of(contract));
            _contractRepoMock.Setup(x => x.IsActiveContract(command.ContractId)).ReturnsAsync(true);
            _googleApiServiceMock.Setup(x => x.GetCoordinatesFromAddress(command.IncidentAddress)).ReturnsAsync(Result<Core.Infrastructure.GoogleMaps.Coordinates>.Success(new Core.Infrastructure.GoogleMaps.Coordinates { Latitude = 10.0, Longitude = 10.0 }));
            _googleApiServiceMock.Setup(x => x.GetCoordinatesFromAddress(command.DestinationAddress)).ReturnsAsync(Result<Core.Infrastructure.GoogleMaps.Coordinates>.Success(new Core.Infrastructure.GoogleMaps.Coordinates { Latitude = 10.0, Longitude = 10.0 }));
            _idGeneratorMock.Setup(x => x.Generate()).Returns("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a");
            _publishEndpointMock.Setup(x => x.Publish(It.IsAny<OrderCreatedEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _busMock.Setup(x => x.Publish(It.IsAny<CreateOrderCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _controller.CreateOrder(command, token);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, actionResult.StatusCode);
            Assert.Equal(new { id = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a" }, actionResult.Value);
        }

        [Fact]
        public async Task CreateOrder_ShouldReturn400_WhenValidationFails()
        {
            var command = new CreateOrderCommand(
                "53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d",
                "53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a",
                "47c0d8fa-dbca-4d98-9fdf-1d1413e90f00",
                "El paraiso, caracas",
                "Altamira, caracas",
                "",
                new List<string>() { }
            );

            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeIeyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeI";
            var restResponse = new RestResponse
            {
                Content = "{\"id\":\"53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a\",\"name\":\"Operator\",\"email\":\"operator@gmail.com\",\"phone\":\"+58 424-2720504\",\"userType\":\"Operator\",\"isActive\":true,\"department\":\"Service\",\"isTemporaryPassword\":false,\"passwordExpirationDate\":\"2125-11-01T23:25:59\"}",
                StatusCode = System.Net.HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed,
                ContentType = "application/json",
                IsSuccessStatusCode = true,
                ResponseUri = new Uri("https://localhost:4051/user/53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                StatusDescription = "OK"
            };

            _restClientMock.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(restResponse);
            var validationResult = new FluentValidation.Results.ValidationResult(new List<ValidationFailure> { new ValidationFailure("IncidentType", "Incident type is required") });
            _validatorCreateMock.Setup(x => x.Validate(command)).Returns(validationResult);


            var result = await _controller.CreateOrder(command, token) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result?.StatusCode);
            Assert.Equal(new List<string> { "Incident type is required" }, result?.Value);
        }

        [Fact]
        public async Task CreateExtraCost_Success()
        {
            var command = new CreateExtraCostCommand(
                "53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r",
                new List<ExtraCostDto2>() { }
            );
            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeIeyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeI";

            var id = new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r");
            var contractId = new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d");
            var operatorId = new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a");
            var driverAssigned = new DriverId("Por asignar");
            var incidentAddress = new Coordinates(10.0, 10.0);
            var destinationAddress = new Coordinates(11.0, 11.0);
            var type = new IncidentType("Fallo de Frenos");
            var incidentDate = DateTime.UtcNow;
            var extraCosts = new List<ExtraCost>();
            var totalCost = new TotalCost(10);
            var status = new OrderStatus("Por Aceptar");
            var existingOrder = Order.CreateOrder(id, contractId, operatorId, driverAssigned, incidentAddress, destinationAddress, type, incidentDate, extraCosts, totalCost, status);

            var extraCost = new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d2r"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r"), new ExtraCostName("Cambio de Neumático"), new ExtraCostPrice(5));

            _validatorCreateExtraCostMock.Setup(v => v.Validate(It.IsAny<CreateExtraCostCommand>())).Returns(new FluentValidation.Results.ValidationResult());
            _orderRepoMock.Setup(r => r.GetById(id.GetValue())).ReturnsAsync(Optional<Order>.Of(existingOrder));
            _extraCostRepoMock.Setup(r => r.Save(extraCost)).ReturnsAsync(Result<ExtraCost>.Success(extraCost));
            _idGeneratorMock.Setup(x => x.Generate()).Returns("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a");

            var result = await _controller.CreateExtraCost(command, token);

            var actionResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(201, actionResult.StatusCode);
        }

        [Fact]
        public async Task CreateExtraCost_ShouldReturn400_WhenValidationFails()
        {
            var command = new CreateExtraCostCommand(
                "53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r",
                new List<ExtraCostDto2>() { }
            );
            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeIeyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeI";


            var validationResult = new FluentValidation.Results.ValidationResult(new List<ValidationFailure> { new ValidationFailure("OrderId", "Order ID is required.") });
            _validatorCreateExtraCostMock.Setup(x => x.Validate(command)).Returns(validationResult);


            var result = await _controller.CreateExtraCost(command, token) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result?.StatusCode);
            Assert.Equal(new List<string> { "Order ID is required." }, result?.Value);
        }

        [Fact]
        public async Task GetExtraCostByOrderId_ShouldReturn200_WhenExtraCostsAreRetrievedSuccessfully()
        {
            var extraCosts = new List<ExtraCost>
            {
                new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d2r"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r"), new ExtraCostName("Cambio de Neumático"), new ExtraCostPrice(5)),
                new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d1w"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r"), new ExtraCostName("Desbloqueo del vehículo"), new ExtraCostPrice(2))
            };

            var id = new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r");
            var contractId = new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d");
            var operatorId = new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a");
            var driverAssigned = new DriverId("Por asignar");
            var incidentAddress = new Coordinates(10.0, 10.0);
            var destinationAddress = new Coordinates(11.0, 11.0);
            var type = new IncidentType("Fallo de Frenos");
            var incidentDate = DateTime.UtcNow;
            var totalCost = new TotalCost(10);
            var status = new OrderStatus("Por Aceptar");
            var existingOrder = Order.CreateOrder(id, contractId, operatorId, driverAssigned, incidentAddress, destinationAddress, type, incidentDate, extraCosts, totalCost, status);

            var getExtraCostResponse = new GetExtraCostResponse(extraCosts.Select(ec => new ExtraCostDto(ec.GetId(), ec.GetName(), ec.GetPrice())).ToList());

            _extraCostRepoMock.Setup(x => x.GetExtraCostByOrderId(id.GetValue())).ReturnsAsync(extraCosts);
            _orderRepoMock.Setup(x => x.GetById(id.GetValue())).ReturnsAsync(Optional<Order>.Of(existingOrder));

            var response = await _controller.GetExtraCostByOrderId(id.GetValue());

            var actionResult = Assert.IsType<ObjectResult>(response);
            Assert.Equal(200, actionResult.StatusCode);
            var responseValue = Assert.IsType<GetExtraCostResponse>(actionResult.Value);
            Assert.Equal(getExtraCostResponse.ExtraCosts.Count, responseValue.ExtraCosts.Count);
            Assert.Equal(getExtraCostResponse.ExtraCosts[0].Id, responseValue.ExtraCosts[0].Id);
            Assert.Equal(getExtraCostResponse.ExtraCosts[1].Id, responseValue.ExtraCosts[1].Id);
        }


        [Fact]
        public async Task GetAllOrders_ShouldReturn200_WhenOrdersAreRetrievedSuccessfully()
        {
            var query = new GetAllOrdersQuery(10, 1, "");
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
                    new List<ExtraCost>
                    {
                        new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d2r"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r"), new ExtraCostName("Cambio de Neumático"), new ExtraCostPrice(5)),
                        new ExtraCost(new ExtraCostId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d1w"), new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d7r"), new ExtraCostName("Desbloqueo del vehículo"), new ExtraCostPrice(2))
                    },
                    new TotalCost(80),
                    new OrderStatus("Pagado")
                )
            };

            var orderResponses = orders.Select(o => new GetOrderResponse(
                o.GetId(),
                o.GetContractId(),
                o.GetOperatorAssigned(),
                o.GetDriverAssigned(),
                new CoordinatesDto(o.GetIncidentAddressLatitude(), o.GetIncidentAddressLongitude()),
                new CoordinatesDto(o.GetDestinationAddressLatitude(), o.GetDestinationAddressLongitude()),
                o.GetIncidentType(),
                o.GetIncidentDate(),
                o.GetExtrasServicesApplied().Select(extraCost => new ExtraServiceDto(
                    extraCost.GetId(),
                    extraCost.GetName(),
                    extraCost.GetPrice()
                )).ToList(),
                o.GetTotalCost(),
                o.GetOrderStatus()
            )).ToArray();

            _orderRepoMock.Setup(x => x.GetAll(query)).ReturnsAsync(orders);
            var result = await _controller.GetAllOrders(query);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);

            var responseValue = Assert.IsType<GetOrderResponse[]>(actionResult.Value);

            for (int i = 0; i < orderResponses.Length; i++)
            {
                Assert.Equal(orderResponses[i].Id, responseValue[i].Id);
                Assert.Equal(orderResponses[i].ContractClient, responseValue[i].ContractClient);
                Assert.Equal(orderResponses[i].OperatorAssigned, responseValue[i].OperatorAssigned);
                Assert.Equal(orderResponses[i].DriverAssigned, responseValue[i].DriverAssigned);
                Assert.Equal(orderResponses[i].IncidentAddress.Latitude, responseValue[i].IncidentAddress.Latitude);
                Assert.Equal(orderResponses[i].IncidentAddress.Longitude, responseValue[i].IncidentAddress.Longitude);
                Assert.Equal(orderResponses[i].DestinationAddress.Latitude, responseValue[i].DestinationAddress.Latitude);
                Assert.Equal(orderResponses[i].DestinationAddress.Longitude, responseValue[i].DestinationAddress.Longitude);
                Assert.Equal(orderResponses[i].IncidentType, responseValue[i].IncidentType);
                Assert.Equal(orderResponses[i].IncidentDate, responseValue[i].IncidentDate);
                Assert.Equal(orderResponses[i].TotalCost, responseValue[i].TotalCost);
                Assert.Equal(orderResponses[i].Status, responseValue[i].Status);
            }
        }

        [Fact]
        public async Task GetAllOrdersByDriverAssigned_ShouldReturn200_WhenOrdersAreRetrievedSuccessfully()
        {
            var driverId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a";
            var query = new GetAllOrdersByDriverAssignedQuery(10, 1);
            var orders = new List<Order>
            {
                Order.CreateOrder(
                    new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a"),
                    new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                    new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                    new DriverId(driverId),
                    new Coordinates(10.0, 10.0),
                    new Coordinates(11.0, 11.0),
                    new IncidentType("Fallo de Frenos"),
                    DateTime.UtcNow,
                    new List<ExtraCost>(),
                    new TotalCost(10),
                    new OrderStatus("Por Aceptar")
                )
            };

            var orderResponses = orders.Select(o => new GetOrderResponse(
                o.GetId(),
                o.GetContractId(),
                o.GetOperatorAssigned(),
                o.GetDriverAssigned(),
                new CoordinatesDto(o.GetIncidentAddressLatitude(), o.GetIncidentAddressLongitude()),
                new CoordinatesDto(o.GetDestinationAddressLatitude(), o.GetDestinationAddressLongitude()),
                o.GetIncidentType(),
                o.GetIncidentDate(),
                o.GetExtrasServicesApplied().Select(extraCost => new ExtraServiceDto(
                    extraCost.GetId(),
                    extraCost.GetName(),
                    extraCost.GetPrice()
                )).ToList(),
                o.GetTotalCost(),
                o.GetOrderStatus()
            )).ToArray();

            _orderRepoMock.Setup(x => x.GetAllOrdersByDriverAssigned(query, driverId)).ReturnsAsync(orders);

            var result = await _controller.GetAllOrdersByDriverAssigned(query, driverId);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);

            var responseValue = Assert.IsType<GetOrderResponse[]>(actionResult.Value);
            Assert.Equal(orderResponses.Length, responseValue.Length);
            for (int i = 0; i < orderResponses.Length; i++)
            {
                Assert.Equal(orderResponses[i].Id, responseValue[i].Id);
                Assert.Equal(orderResponses[i].ContractClient, responseValue[i].ContractClient);
                Assert.Equal(orderResponses[i].OperatorAssigned, responseValue[i].OperatorAssigned);
                Assert.Equal(orderResponses[i].DriverAssigned, responseValue[i].DriverAssigned);
                Assert.Equal(orderResponses[i].IncidentAddress.Latitude, responseValue[i].IncidentAddress.Latitude);
                Assert.Equal(orderResponses[i].IncidentAddress.Longitude, responseValue[i].IncidentAddress.Longitude);
                Assert.Equal(orderResponses[i].DestinationAddress.Latitude, responseValue[i].DestinationAddress.Latitude);
                Assert.Equal(orderResponses[i].DestinationAddress.Longitude, responseValue[i].DestinationAddress.Longitude);
                Assert.Equal(orderResponses[i].IncidentType, responseValue[i].IncidentType);
                Assert.Equal(orderResponses[i].IncidentDate, responseValue[i].IncidentDate);
                Assert.Equal(orderResponses[i].TotalCost, responseValue[i].TotalCost);
                Assert.Equal(orderResponses[i].Status, responseValue[i].Status);
            }
        }

        [Fact]
        public async Task GetAllOrdersByDriverAssigned_ShouldReturnEmptyArray_WhenExceptionIsThrown()
        {
            var driverId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a";
            var query = new GetAllOrdersByDriverAssignedQuery(10, 1);

            _orderRepoMock.Setup(x => x.GetAllOrdersByDriverAssigned(query, driverId)).ThrowsAsync(new Exception("Database error"));

            var result = await _controller.GetAllOrdersByDriverAssigned(query, driverId);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
            Assert.Empty((GetOrderResponse[])actionResult.Value);
        }

        [Fact]
        public async Task GetOrderById_ShouldReturn200_WhenOrderExist()
        {
            var id = new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a");
            var contractId = new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d");
            var operatorId = new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a");
            var driverAssigned = new DriverId("Por asignar");
            var incidentAddress = new Coordinates(10.0, 10.0);
            var destinationAddress = new Coordinates(11.0, 11.0);
            var type = new IncidentType("Fallo de Frenos");
            var incidentDate = DateTime.UtcNow;
            var extraCosts = new List<ExtraCost>();
            var totalCost = new TotalCost(10);
            var status = new OrderStatus("Por Aceptar");
            var existingOrder = Order.CreateOrder(id, contractId, operatorId, driverAssigned, incidentAddress, destinationAddress, type, incidentDate, extraCosts, totalCost, status);

            var query = new GetOrderByIdQuery(driverAssigned.GetValue());

            _orderRepoMock.Setup(r => r.GetById(id.GetValue())).ReturnsAsync(Optional<Order>.Of(existingOrder));

            var result = await _controller.GetOrderById(id.GetValue());

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
        }

        [Fact]
        public async Task GetOrderById_ShouldReturn500_WhenOrderNotFound()
        {
            var query = new GetOrderByIdQuery("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");

            _orderRepoMock.Setup(r => r.GetById("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e")).ReturnsAsync(Optional<Order>.Empty());

            var result = await _controller.GetOrderById(query.Id);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, actionResult.StatusCode);
        }

        [Fact]
        public async Task AddExtraCost_ShouldReturn200_WhenAddedIsSuccessful()
        {
            var id = new OrderId("53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a");
            var contractId = new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d");
            var operatorId = new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a");
            var driverAssigned = new DriverId("Por asignar");
            var incidentAddress = new Coordinates(10.0, 10.0);
            var destinationAddress = new Coordinates(11.0, 11.0);
            var type = new IncidentType("Fallo de Frenos");
            var incidentDate = DateTime.UtcNow;
            var extraCosts = new List<ExtraCost>();
            var totalCost = new TotalCost(10);
            var status = new OrderStatus("Por Aceptar");
            var existingOrder = Order.CreateOrder(id, contractId, operatorId, driverAssigned, incidentAddress, destinationAddress, type, incidentDate, extraCosts, totalCost, status);

            var command = new ValidatePricesOfExtrasCostCommand(true, new List<ExtraCostDto>());
            var extraServices = existingOrder.GetExtrasServicesApplied().Select(extraCost => new ExtraServiceDto(
                extraCost.GetId(),
                extraCost.GetName(),
                extraCost.GetPrice()
            )).ToList();

            var addExtraCostResult = Result<GetOrderResponse>.Success(new GetOrderResponse(
                existingOrder.GetId(),
                existingOrder.GetContractId(),
                existingOrder.GetOperatorAssigned(),
                existingOrder.GetDriverAssigned(),
                new CoordinatesDto(existingOrder.GetIncidentAddressLatitude(), existingOrder.GetIncidentAddressLongitude()),
                new CoordinatesDto(existingOrder.GetDestinationAddressLatitude(), existingOrder.GetDestinationAddressLongitude()),
                existingOrder.GetIncidentType(),
                existingOrder.GetIncidentDate(),
                extraServices,
                existingOrder.GetTotalCost(),
                existingOrder.GetOrderStatus())
            );

            _validatorPricesExtrasCostMock.Setup(x => x.Validate(command)).Returns(new FluentValidation.Results.ValidationResult());
            _orderRepoMock.Setup(x => x.GetById(id.GetValue())).ReturnsAsync(Optional<Order>.Of(existingOrder));
            _addExtraCostMock.Setup(x => x.Execute(It.IsAny<(string, AddExtraCostCommand)>())).ReturnsAsync(addExtraCostResult);
            _orderRepoMock.Setup(x => x.Update(It.IsAny<Order>())).ReturnsAsync(Result<Order>.Success(existingOrder));

            var result = await _controller.AddExtraCost(command, id.GetValue());

            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
        }

        [Fact]
        public async Task AddExtraCost_ShouldReturn400_WhenValidationFails()
        {
            var command = new ValidatePricesOfExtrasCostCommand(true, new List<ExtraCostDto>());
            var validationResult = new FluentValidation.Results.ValidationResult(new List<ValidationFailure> { new ValidationFailure("OperatorRespose", "Response of operator must be true or false.") });
            _validatorPricesExtrasCostMock.Setup(x => x.Validate(command)).Returns(validationResult);

            var result = await _controller.AddExtraCost(command, "53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e") as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result?.StatusCode);
            Assert.Equal(new List<string> { "Response of operator must be true or false." }, result?.Value);
        }

        [Fact]
        public async Task UpdateDriverAssigned_ShouldReturnOk_WhenUpdateIsSuccessful()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new UpdateDriverAssignedCommand("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a");
            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeI";

            var driverResponseContent = "{\"id\":\"53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a\",\"name\":\"Driver\",\"email\":\"driver@gmail.com\",\"phone\":\"+58 424-2720504\",\"userType\":\"Driver\",\"isActive\":true,\"department\":\"Service\",\"isTemporaryPassword\":false,\"passwordExpirationDate\":\"2125-11-01T23:25:59\"}";
            var availableDriversResponseContent = "[{\"id\":\"53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a\",\"name\":\"Driver\",\"isAvailable\":true}]";

            var restResponseDriver = new RestResponse
            {
                Content = driverResponseContent,
                StatusCode = System.Net.HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed,
                ContentType = "application/json",
                IsSuccessStatusCode = true,
                ResponseUri = new Uri($"https://localhost:4052/provider/driver/{command.DriverAssigned}"),
                StatusDescription = "OK"
            };

            var restResponseAvailableDrivers = new RestResponse
            {
                Content = availableDriversResponseContent,
                StatusCode = System.Net.HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed,
                ContentType = "application/json",
                IsSuccessStatusCode = true,
                ResponseUri = new Uri("https://localhost:4052/provider/provider/availables"),
                StatusDescription = "OK"
            };

            var existingOrder = Order.CreateOrder(
                new OrderId(orderId),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new DriverId("Por asignar"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(10),
                new OrderStatus("Por Asignar")
            );

            var updateDriverAssignedResult = Result<GetOrderResponse>.Success(new GetOrderResponse(
                existingOrder.GetId(),
                existingOrder.GetContractId(),
                existingOrder.GetOperatorAssigned(),
                command.DriverAssigned,
                new CoordinatesDto(existingOrder.GetIncidentAddressLatitude(), existingOrder.GetIncidentAddressLongitude()),
                new CoordinatesDto(existingOrder.GetDestinationAddressLatitude(), existingOrder.GetDestinationAddressLongitude()),
                existingOrder.GetIncidentType(),
                existingOrder.GetIncidentDate(),
                existingOrder.GetExtrasServicesApplied().Select(extraCost => new ExtraServiceDto(
                    extraCost.GetId(),
                    extraCost.GetName(),
                    extraCost.GetPrice()
                )).ToList(),
                existingOrder.GetTotalCost(),
                existingOrder.GetOrderStatus())
            );

            _restClientMock.Setup(x => x.ExecuteAsync(It.Is<RestRequest>(r => r.Resource.Contains("provider/driver")), It.IsAny<CancellationToken>())).ReturnsAsync(restResponseDriver);
            _restClientMock.Setup(x => x.ExecuteAsync(It.Is<RestRequest>(r => r.Resource.Contains("provider/availables")), It.IsAny<CancellationToken>())).ReturnsAsync(restResponseAvailableDrivers);
            _validatorDriverAssignedMock.Setup(x => x.Validate(command)).Returns(new FluentValidation.Results.ValidationResult());
            _orderRepoMock.Setup(x => x.GetById(orderId)).ReturnsAsync(Optional<Order>.Of(existingOrder));
            _orderRepoMock.Setup(x => x.Update(It.IsAny<Order>())).ReturnsAsync(Result<Order>.Success(existingOrder));
            _publishEndpointMock.Setup(x => x.Publish(It.IsAny<UpdateDriverAssignedCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _controller.UpdateDriverAssigned(command, orderId, token);

            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
            var responseValue = Assert.IsType<GetOrderResponse>(actionResult.Value);
            Assert.Equal(command.DriverAssigned, responseValue.DriverAssigned);
        }

        [Fact]
        public async Task UpdateDriverAssigned_ShouldReturn400_WhenValidationFails()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new UpdateDriverAssignedCommand("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a");
            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeI";

            var driverResponseContent = "{\"id\":\"53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a\",\"name\":\"Driver\",\"email\":\"driver@gmail.com\",\"phone\":\"+58 424-2720504\",\"userType\":\"Driver\",\"isActive\":true,\"department\":\"Service\",\"isTemporaryPassword\":false,\"passwordExpirationDate\":\"2125-11-01T23:25:59\"}";
            var availableDriversResponseContent = "[{\"id\":\"53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a\",\"name\":\"Driver\",\"isAvailable\":false}]";

            var restResponseDriver = new RestResponse
            {
                Content = driverResponseContent,
                StatusCode = System.Net.HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed,
                ContentType = "application/json",
                IsSuccessStatusCode = true,
                ResponseUri = new Uri($"https://localhost:4052/provider/driver/{command.DriverAssigned}"),
                StatusDescription = "OK"
            };

            var restResponseAvailableDrivers = new RestResponse
            {
                Content = availableDriversResponseContent,
                StatusCode = System.Net.HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed,
                ContentType = "application/json",
                IsSuccessStatusCode = true,
                ResponseUri = new Uri("https://localhost:4052/provider/provider/availables"),
                StatusDescription = "OK"
            };

            _restClientMock.Setup(x => x.ExecuteAsync(It.Is<RestRequest>(r => r.Resource.Contains("provider/driver")), It.IsAny<CancellationToken>())).ReturnsAsync(restResponseDriver);
            _restClientMock.Setup(x => x.ExecuteAsync(It.Is<RestRequest>(r => r.Resource.Contains("provider/availables")), It.IsAny<CancellationToken>())).ReturnsAsync(restResponseAvailableDrivers);
            var validationResult = new FluentValidation.Results.ValidationResult(new List<ValidationFailure> { new ValidationFailure("DriverAssigned", "Driver ID is required.") });
            _validatorDriverAssignedMock.Setup(x => x.Validate(command)).Returns(validationResult);

            var result = await _controller.UpdateDriverAssigned(command, orderId, token) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result?.StatusCode);
            Assert.Equal(new List<string> { "Driver ID is required." }, result?.Value);
        }

        [Fact]
        public async Task UpdateDriverAssigned_ShouldReturn500_WhenOrderNotFound()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new UpdateDriverAssignedCommand("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a");
            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeI";

            var driverResponseContent = "{\"id\":\"53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a\",\"name\":\"Driver\",\"email\":\"driver@gmail.com\",\"phone\":\"+58 424-2720504\",\"userType\":\"Driver\",\"isActive\":true,\"department\":\"Service\",\"isTemporaryPassword\":false,\"passwordExpirationDate\":\"2125-11-01T23:25:59\"}";
            var availableDriversResponseContent = "[{\"id\":\"53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a\",\"name\":\"Driver\",\"isAvailable\":false}]";

            var restResponseDriver = new RestResponse
            {
                Content = driverResponseContent,
                StatusCode = System.Net.HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed,
                ContentType = "application/json",
                IsSuccessStatusCode = true,
                ResponseUri = new Uri($"https://localhost:4052/provider/driver/{command.DriverAssigned}"),
                StatusDescription = "OK"
            };

            var restResponseAvailableDrivers = new RestResponse
            {
                Content = availableDriversResponseContent,
                StatusCode = System.Net.HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed,
                ContentType = "application/json",
                IsSuccessStatusCode = true,
                ResponseUri = new Uri("https://localhost:4052/provider/provider/availables"),
                StatusDescription = "OK"
            };

            _restClientMock.Setup(x => x.ExecuteAsync(It.Is<RestRequest>(r => r.Resource.Contains("provider/driver")), It.IsAny<CancellationToken>())).ReturnsAsync(restResponseDriver);
            _restClientMock.Setup(x => x.ExecuteAsync(It.Is<RestRequest>(r => r.Resource.Contains("provider/availables")), It.IsAny<CancellationToken>())).ReturnsAsync(restResponseAvailableDrivers);
            _validatorDriverAssignedMock.Setup(x => x.Validate(command)).Returns(new FluentValidation.Results.ValidationResult());
            _orderRepoMock.Setup(x => x.GetById(orderId)).ReturnsAsync(Optional<Order>.Empty());

            var result = await _controller.UpdateDriverAssigned(command, "53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e", token);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, actionResult.StatusCode);
        }

        [Fact]
        public async Task UpdateOrderStatus_ShouldReturnOk_WhenDriverAcceptedOrder()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new UpdateOrderStatusCommand(true, false, false);
            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeI";

            var existingOrder = Order.CreateOrder(
                new OrderId(orderId),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f6t"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(10),
                new OrderStatus("Por Aceptar")
            );

            var restResponseDriver = new RestResponse
            {
                Content = "{\"id\":\"53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a\",\"isAvailable\":false}",
                StatusCode = System.Net.HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed,
                ContentType = "application/json",
                IsSuccessStatusCode = true,
                ResponseUri = new Uri($"https://localhost:4052/provider/driver/{existingOrder.GetDriverAssigned()}"),
                StatusDescription = "OK"
            };

            _validatorOrderStatusMock.Setup(v => v.Validate(It.IsAny<UpdateOrderStatusCommand>())).Returns(new FluentValidation.Results.ValidationResult());
            _orderRepoMock.Setup(x => x.GetById(orderId)).ReturnsAsync(Optional<Order>.Of(existingOrder));
            _orderRepoMock.Setup(r => r.Update(existingOrder)).ReturnsAsync(Result<Order>.Success(existingOrder));
            _publishEndpointMock.Setup(x => x.Publish(It.IsAny<DriverAcceptedOrderEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _restClientMock.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(restResponseDriver);

            var result = await _controller.UpdateOrderStatus(command, orderId, token);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task UpdateOrderStatus_ShouldReturnOk_WhenDriverRefusedOrder()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new UpdateOrderStatusCommand(false, false, false);
            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeI";

            var existingOrder = Order.CreateOrder(
                new OrderId(orderId),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f6t"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(10),
                new OrderStatus("Por Aceptar")
            );

            _validatorOrderStatusMock.Setup(v => v.Validate(It.IsAny<UpdateOrderStatusCommand>())).Returns(new FluentValidation.Results.ValidationResult());
            _orderRepoMock.Setup(x => x.GetById(orderId)).ReturnsAsync(Optional<Order>.Of(existingOrder));
            _orderRepoMock.Setup(r => r.Update(existingOrder)).ReturnsAsync(Result<Order>.Success(existingOrder));
            _publishEndpointMock.Setup(x => x.Publish(It.IsAny<DriverAcceptedOrderEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _controller.UpdateOrderStatus(command, orderId, token);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task UpdateOrderStatus_ShouldReturnOk_WhenOrderIsInPorgress()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new UpdateOrderStatusCommand(null, true, false);
            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeI";

            var existingOrder = Order.CreateOrder(
                new OrderId(orderId),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f6t"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(10),
                new OrderStatus("Localizado")
            );

            _validatorOrderStatusMock.Setup(v => v.Validate(It.IsAny<UpdateOrderStatusCommand>())).Returns(new FluentValidation.Results.ValidationResult());
            _orderRepoMock.Setup(x => x.GetById(orderId)).ReturnsAsync(Optional<Order>.Of(existingOrder));
            _orderRepoMock.Setup(r => r.Update(existingOrder)).ReturnsAsync(Result<Order>.Success(existingOrder));
            _publishEndpointMock.Setup(x => x.Publish(It.IsAny<OrderInProcessEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _controller.UpdateOrderStatus(command, orderId, token);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task UpdateOrderStatus_ShouldReturnOk_WhenDriverCanceledOrder()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new UpdateOrderStatusCommand(null, false, true);
            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeI";

            var existingOrder = Order.CreateOrder(
                new OrderId(orderId),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f6t"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(10),
                new OrderStatus("Localizado")
            );

            var restResponseDriver = new RestResponse
            {
                Content = "{\"id\":\"53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a\",\"isAvailable\":true}",
                StatusCode = System.Net.HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed,
                ContentType = "application/json",
                IsSuccessStatusCode = true,
                ResponseUri = new Uri($"https://localhost:4052/provider/driver/{existingOrder.GetDriverAssigned()}"),
                StatusDescription = "OK"
            };

            _validatorOrderStatusMock.Setup(v => v.Validate(It.IsAny<UpdateOrderStatusCommand>())).Returns(new FluentValidation.Results.ValidationResult());
            _orderRepoMock.Setup(x => x.GetById(orderId)).ReturnsAsync(Optional<Order>.Of(existingOrder));
            _orderRepoMock.Setup(r => r.Update(existingOrder)).ReturnsAsync(Result<Order>.Success(existingOrder));
            _publishEndpointMock.Setup(x => x.Publish(It.IsAny<OrderInProcessEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _restClientMock.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(restResponseDriver);

            var result = await _controller.UpdateOrderStatus(command, orderId, token);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task UpdateOrderStatus_ShouldReturn400_WhenValidationFails()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new UpdateOrderStatusCommand(true, false, false);
            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeI";

            var existingOrder = Order.CreateOrder(
                new OrderId(orderId),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f6t"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(10),
                new OrderStatus("Por Aceptar")
            );

            var validationResult = new FluentValidation.Results.ValidationResult(new List<ValidationFailure> { new ValidationFailure("OrderAcceptedDriverResponse", "Driver response of order accepted must be true or false.") });
            _validatorOrderStatusMock.Setup(x => x.Validate(command)).Returns(validationResult);

            var result = await _controller.UpdateOrderStatus(command, orderId, token) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result?.StatusCode);
            Assert.Equal(new List<string> { "Driver response of order accepted must be true or false." }, result?.Value);
        }

        [Fact]
        public async Task UpdateOrderStatusToCompleted_ShouldReturnOk()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new UpdateOrderStatusToCompletedCommand(true);
            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeI";

            var existingOrder = Order.CreateOrder(
                new OrderId(orderId),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f6t"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(10),
                new OrderStatus(OrderStatus.EnProceso)
            );

            var restResponseDriver1 = new RestResponse
            {
                Content = "{\"isAvailable\":true}",
                StatusCode = System.Net.HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed,
                ContentType = "application/json",
                IsSuccessStatusCode = true,
                ResponseUri = new Uri($"https://localhost:4052/provider/driver/{existingOrder.GetDriverAssigned()}"),
                StatusDescription = "OK"
            };

            var restResponseDriver2 = new RestResponse
            {
                Content = "{\"latitude\":11.0,\"longitude\":11.0}",
                StatusCode = System.Net.HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed,
                ContentType = "application/json",
                IsSuccessStatusCode = true,
                ResponseUri = new Uri($"https://localhost:4052/provider/driver/{existingOrder.GetDriverAssigned()}"),
                StatusDescription = "OK"
            };

            _validatorOrderstatusToCompletedMock.Setup(v => v.Validate(It.IsAny<UpdateOrderStatusToCompletedCommand>())).Returns(new FluentValidation.Results.ValidationResult());
            _orderRepoMock.Setup(x => x.GetById(orderId)).ReturnsAsync(Optional<Order>.Of(existingOrder));
            _orderRepoMock.Setup(r => r.Update(existingOrder)).ReturnsAsync(Result<Order>.Success(existingOrder));
            _publishEndpointMock.Setup(x => x.Publish(It.IsAny<DriverAcceptedOrderEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _restClientMock.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(restResponseDriver1);
            _restClientMock.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(restResponseDriver2);

            var result = await _controller.UpdateOrderStatusToCompleted(command, orderId, token);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task UpdateOrderStatusToCompleted_ShouldReturn400_WhenValidationFails()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new UpdateOrderStatusToCompletedCommand(true);
            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeI";

            var existingOrder = Order.CreateOrder(
                new OrderId(orderId),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f6t"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(10),
                new OrderStatus(OrderStatus.EnProceso)
            );

            var validationResult = new FluentValidation.Results.ValidationResult(new List<ValidationFailure> { new ValidationFailure("OrderCompletedDriverResponse", "Driver response of order completed must be true or false.") });
            _validatorOrderstatusToCompletedMock.Setup(x => x.Validate(command)).Returns(validationResult);

            var result = await _controller.UpdateOrderStatusToCompleted(command, orderId, token) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result?.StatusCode);
            Assert.Equal(new List<string> { "Driver response of order completed must be true or false." }, result?.Value);
        }

        [Fact]
        public async Task UpdateOrderStatusToCompleted_ShouldReturn500_WhenDriverAvailabilityUpdateFails()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new UpdateOrderStatusToCompletedCommand(true);
            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeI";

            var existingOrder = Order.CreateOrder(
                new OrderId(orderId),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f6t"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(10),
                new OrderStatus(OrderStatus.EnProceso)
            );

            _validatorOrderstatusToCompletedMock.Setup(v => v.Validate(It.IsAny<UpdateOrderStatusToCompletedCommand>())).Returns(new FluentValidation.Results.ValidationResult());
            _orderRepoMock.Setup(x => x.GetById(orderId)).ReturnsAsync(Optional<Order>.Of(existingOrder));
            _orderRepoMock.Setup(r => r.Update(existingOrder)).ReturnsAsync(Result<Order>.Success(existingOrder));
            _publishEndpointMock.Setup(x => x.Publish(It.IsAny<DriverAcceptedOrderEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _restClientMock.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(new RestResponse { IsSuccessStatusCode = false, Content = "Failed to update driver availability" });

            var result = await _controller.UpdateOrderStatusToCompleted(command, orderId, token);

            var internalServerErrorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, internalServerErrorResult.StatusCode);
            Assert.Equal("Failed to update driver availability. Content: Failed to update driver availability", internalServerErrorResult.Value);
        }

        [Fact]
        public async Task UpdateOrderStatusToCompleted_ShouldReturn500_WhenDriverLocationUpdateFails()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new UpdateOrderStatusToCompletedCommand(true);
            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeI";

            var existingOrder = Order.CreateOrder(
                new OrderId(orderId),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f6t"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(10),
                new OrderStatus(OrderStatus.EnProceso)
            );

            var restResponseDriver1 = new RestResponse
            {
                Content = "{\"isAvailable\":true}",
                StatusCode = System.Net.HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed,
                ContentType = "application/json",
                IsSuccessStatusCode = true,
                ResponseUri = new Uri($"https://localhost:4052/provider/driver/{existingOrder.GetDriverAssigned()}"),
                StatusDescription = "OK"
            };

            _validatorOrderstatusToCompletedMock.Setup(v => v.Validate(It.IsAny<UpdateOrderStatusToCompletedCommand>())).Returns(new FluentValidation.Results.ValidationResult());
            _orderRepoMock.Setup(x => x.GetById(orderId)).ReturnsAsync(Optional<Order>.Of(existingOrder));
            _orderRepoMock.Setup(r => r.Update(existingOrder)).ReturnsAsync(Result<Order>.Success(existingOrder));
            _publishEndpointMock.Setup(x => x.Publish(It.IsAny<DriverAcceptedOrderEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _restClientMock.SetupSequence(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(restResponseDriver1)
                .ReturnsAsync(new RestResponse { IsSuccessStatusCode = false, Content = "Failed to update driver location" });

            var result = await _controller.UpdateOrderStatusToCompleted(command, orderId, token);

            var internalServerErrorResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, internalServerErrorResult.StatusCode);
            Assert.Equal("Failed to update driver location. Content: Failed to update driver location", internalServerErrorResult.Value);
        }

        [Fact]
        public async Task UpdateOrderStatusToPaid_ShouldReturnOk_WhenUpdateIsSuccessful()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new UpdateOrderStatusToPaidCommand(true);
            var order = Order.CreateOrder(
                new OrderId(orderId),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new DriverId("Por asignar"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(10),
                new OrderStatus("Finalizado")
            );

            _validatorOrderstatusToPaidMock.Setup(v => v.Validate(It.IsAny<UpdateOrderStatusToPaidCommand>())).Returns(new FluentValidation.Results.ValidationResult());
            _orderRepoMock.Setup(x => x.GetById(orderId)).ReturnsAsync(Optional<Order>.Of(order));
            _orderRepoMock.Setup(r => r.Update(order)).ReturnsAsync(Result<Order>.Success(order));
            _publishEndpointMock.Setup(x => x.Publish(It.IsAny<UpdateOrderStatusToPaidCommand>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await _controller.UpdateOrderStatusToPaid(command, orderId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public async Task UpdateOrderStatusToPaid_ShouldReturn400_WhenValidationFails()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new UpdateOrderStatusToPaidCommand(true);

            var validationResult = new FluentValidation.Results.ValidationResult(new List<ValidationFailure> { new ValidationFailure("OrderPaidResponse", "Response of order paid must be true or false.") });

            _validatorOrderstatusToPaidMock.Setup(v => v.Validate(It.IsAny<UpdateOrderStatusToPaidCommand>())).Returns(validationResult);

            var result = await _controller.UpdateOrderStatusToPaid(command, orderId);

            var badRequestResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, badRequestResult.StatusCode);
            Assert.Equal(new List<string> { "Response of order paid must be true or false." }, badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateOrderStatusToPaid_ShouldReturn409_WhenUpdateFails()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new UpdateOrderStatusToPaidCommand(true);
            var order = Order.CreateOrder(
                new OrderId(orderId),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new DriverId("Por asignar"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(10),
                new OrderStatus("Finalizado")
            );

            _validatorOrderstatusToPaidMock.Setup(v => v.Validate(It.IsAny<UpdateOrderStatusToPaidCommand>())).Returns(new FluentValidation.Results.ValidationResult());
            _orderRepoMock.Setup(x => x.GetById(orderId)).ReturnsAsync(Optional<Order>.Of(order));
            _orderRepoMock.Setup(r => r.Update(order)).ReturnsAsync(Result<Order>.Failure(new OrderUpdateFailedException("Order status could not be updated correctly")));

            var result = await _controller.UpdateOrderStatusToPaid(command, orderId);

            var conflictResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, conflictResult.StatusCode);
            Assert.Equal("Order status could not be updated correctly", conflictResult.Value);
        }

        [Fact]
        public async Task UpdateOrderStatusToPaid_ShouldReturn409_WhenOrderStatusIsDifferentToCompleted()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new UpdateOrderStatusToPaidCommand(true);
            var order = Order.CreateOrder(
                new OrderId(orderId),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new DriverId("Por asignar"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(10),
                new OrderStatus("Por Asignar")
            );

            _validatorOrderstatusToPaidMock.Setup(v => v.Validate(It.IsAny<UpdateOrderStatusToPaidCommand>())).Returns(new FluentValidation.Results.ValidationResult());
            _orderRepoMock.Setup(x => x.GetById(orderId)).ReturnsAsync(Optional<Order>.Of(order));
            _orderRepoMock.Setup(r => r.Update(order)).ReturnsAsync(Result<Order>.Failure(new OrderUpdateFailedException("The order is not in the Finalizado status")));

            var result = await _controller.UpdateOrderStatusToPaid(command, orderId);

            var conflictResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, conflictResult.StatusCode);
            Assert.Equal("The order is not in the Finalizado status", conflictResult.Value);
        }

        [Fact]
        public async Task UpdateTotalAmountOrder_ShouldReturnOk_WhenUpdateIsSuccessful()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new UpdateTotalAmountOrderCommand(100);
            var order = Order.CreateOrder(
                new OrderId(orderId),
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

            var contract = Contract.CreateContract(
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new ContractNumber(6235),
                new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f80"),
                new VehicleId("47c0d8fa-dbca-4d98-9fdf-1d1413e90f00"),
                DateTime.UtcNow
            );

            var policy = new InsurancePolicy(
                new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f80"),
                new PolicyType("Diamante"),
                new PolicyCoverageKm(25),
                new PolicyIncidentCoverageAmount(50),
                new PriceExtraKm(3)
            );

            _validatorTotalAmountMock.Setup(v => v.Validate(It.IsAny<UpdateTotalAmountOrderCommand>())).Returns(new FluentValidation.Results.ValidationResult());
            _orderRepoMock.Setup(x => x.GetById(orderId)).ReturnsAsync(Optional<Order>.Of(order));
            _orderRepoMock.Setup(x => x.Update(order)).ReturnsAsync(Result<Order>.Success(order));
            _contractRepoMock.Setup(x => x.GetById(contract.GetId())).ReturnsAsync(Optional<Contract>.Of(contract));
            _policyRepoMock.Setup(x => x.GetById(policy.GetId())).ReturnsAsync(Optional<InsurancePolicy>.Of(policy));
            _calculateOrderTotalAmountMock.Setup(x => x.Execute(It.IsAny<CalculateOrderTotalAmountInput>())).Returns(new TotalCost(100));

            var result = await _controller.UpdateTotalAmountOrder(command, orderId);

            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
            var responseValue = Assert.IsType<GetOrderResponse>(actionResult.Value);
            Assert.Equal(orderId, responseValue.Id);
        }

        [Fact]
        public async Task UpdateTotalAmountOrder_ShouldReturn400_WhenValidationFails()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new UpdateTotalAmountOrderCommand(100);
            var validationResult = new FluentValidation.Results.ValidationResult(new List<ValidationFailure> { new ValidationFailure("TotalKmTraveled", "Total Km traveled are required.") });

            _validatorTotalAmountMock.Setup(v => v.Validate(It.IsAny<UpdateTotalAmountOrderCommand>())).Returns(validationResult);

            var result = await _controller.UpdateTotalAmountOrder(command, orderId);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, actionResult.StatusCode);
            Assert.Equal(new List<string> { "Total Km traveled are required." }, actionResult.Value);
        }

        [Fact]
        public async Task UpdateTotalAmountOrder_ShouldReturn409_WhenUpdateFails()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new UpdateTotalAmountOrderCommand(100);
            var order = Order.CreateOrder(
                new OrderId(orderId),
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

            var contract = Contract.CreateContract(
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new ContractNumber(6235),
                new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f80"),
                new VehicleId("47c0d8fa-dbca-4d98-9fdf-1d1413e90f00"),
                DateTime.UtcNow
            );

            var policy = new InsurancePolicy(
                new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f80"),
                new PolicyType("Diamante"),
                new PolicyCoverageKm(25),
                new PolicyIncidentCoverageAmount(50),
                new PriceExtraKm(3)
            );

            _validatorTotalAmountMock.Setup(v => v.Validate(It.IsAny<UpdateTotalAmountOrderCommand>())).Returns(new FluentValidation.Results.ValidationResult());
            _orderRepoMock.Setup(x => x.GetById(orderId)).ReturnsAsync(Optional<Order>.Of(order));
            _orderRepoMock.Setup(x => x.Update(order)).ReturnsAsync(Result<Order>.Success(order));
            _contractRepoMock.Setup(x => x.GetById(contract.GetId())).ReturnsAsync(Optional<Contract>.Of(contract));
            _policyRepoMock.Setup(x => x.GetById(policy.GetId())).ReturnsAsync(Optional<InsurancePolicy>.Of(policy));
            _calculateOrderTotalAmountMock.Setup(x => x.Execute(It.IsAny<CalculateOrderTotalAmountInput>())).Returns(new TotalCost(100));
            _orderRepoMock.Setup(x => x.Update(order)).ReturnsAsync(Result<Order>.Failure(new OrderUpdateFailedException("Order total amount could not be updated correctly")));

            var result = await _controller.UpdateTotalAmountOrder(command, orderId);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, actionResult.StatusCode);
            Assert.Equal("Order total amount could not be updated correctly", actionResult.Value);
        }

        [Fact]
        public async Task ValidateDriverLocation_ShouldReturnOk_WhenUpdateIsSuccessful()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new ValidateLocationCommand(true);
            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeI";
            var order = Order.CreateOrder(
                new OrderId(orderId),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(10),
                new OrderStatus("Aceptado")
            );

            var restResponse = new RestResponse
            {
                Content = "{\"latitude\":10.0,\"longitude\":10.0}",
                StatusCode = System.Net.HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed,
                ContentType = "application/json",
                IsSuccessStatusCode = true,
                ResponseUri = new Uri($"https://localhost:4052/provider/driver/{order.GetDriverAssigned()}/updateLocation"),
                StatusDescription = "OK"
            };

            _validatorLocationDriverMock.Setup(v => v.Validate(It.IsAny<ValidateLocationCommand>())).Returns(new FluentValidation.Results.ValidationResult());
            _orderRepoMock.Setup(x => x.GetById(orderId)).ReturnsAsync(Optional<Order>.Of(order));
            _publishEndpointMock.Setup(x => x.Publish(It.IsAny<DriverIsAtTheIncidentEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            _orderRepoMock.Setup(x => x.Update(order)).ReturnsAsync(Result<Order>.Success(order));
            _restClientMock.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(restResponse);

            var result = await _controller.ValidateDriverLocation(command, orderId, token);

            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
            var responseValue = Assert.IsType<GetOrderResponse>(actionResult.Value);
            Assert.Equal(orderId, responseValue.Id);
        }

        [Fact]
        public async Task ValidateDriverLocation_ShouldReturn409_WhenOrderStatusIsNotAccepted()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new ValidateLocationCommand(true);
            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeI";
            var order = Order.CreateOrder(
                new OrderId(orderId),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(10),
                new OrderStatus("Por Aceptar")
            );

            _validatorLocationDriverMock.Setup(v => v.Validate(It.IsAny<ValidateLocationCommand>())).Returns(new FluentValidation.Results.ValidationResult());
            _orderRepoMock.Setup(x => x.GetById(orderId)).ReturnsAsync(Optional<Order>.Of(order));

            var result = await _controller.ValidateDriverLocation(command, orderId, token);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, actionResult.StatusCode);
            Assert.Equal("The order is not in the Aceptado status", actionResult.Value);
        }

        [Fact]
        public async Task ValidateDriverLocation_ShouldReturn400_WhenValidationFails()
        {
            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var command = new ValidateLocationCommand(true);
            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeI";
            var validationResult = new FluentValidation.Results.ValidationResult(new List<ValidationFailure> { new ValidationFailure("DriverLocationResponse", "Driver response must be true or false.") });

            _validatorLocationDriverMock.Setup(v => v.Validate(It.IsAny<ValidateLocationCommand>())).Returns(validationResult);

            var result = await _controller.ValidateDriverLocation(command, orderId, token);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, actionResult.StatusCode);
            Assert.Equal(new List<string> { "Driver response must be true or false." }, actionResult.Value);
        }

        [Fact]
        public async Task ValidateUpdateTimeDriver_ShouldReturnOk_WhenUpdateIsSuccessful()
        {
            var token = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIwM2NmNTVhYy1jYWZkLTRiZTUtODg1Yy1mNGZjZWMxODE0YzciLCJuYW1lIjoiSm95Y2UiLCJlbWFpbCI6Impld2FnbmVyLjIxQGVzdC51Y2FiLmVkdS52ZSIsInBob25lX251bWJlciI6Iis1OCA0MjQtMjcyMDUwMyIsImlzVGVtcG9yYXJ5UGFzc3dvcmQiOiJmYWxzZSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkFkbWluIiwianRpIjoiNTQ3MjNkMGMtYWNiNS00MTkyLThlODUtZmI1Y2IzNDg5ZWYzIiwiZXhwIjoxNzM2NzA4NjU0LCJpc3MiOiJHcu-_vWFzVUNBQiIsImF1ZCI6Ikdy77-9YXNVQ0FCVXNlcnMifQ.WElef3AbayZCx3nPMn0nxZES-TEGp3eVz0dGHTMpyeI";

            var orderId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90d8a";
            var order = Order.CreateOrder(
                new OrderId(orderId),
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new UserId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new DriverId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5a"),
                new Coordinates(10.0, 10.0),
                new Coordinates(11.0, 11.0),
                new IncidentType("Fallo de Frenos"),
                DateTime.UtcNow,
                new List<ExtraCost>(),
                new TotalCost(10),
                new OrderStatus("Por Aceptar")
            );

            var orders = new List<Order> { order };

            _orderRepoMock.Setup(x => x.ValidateUpdateTimeForStatusPorAceptar()).ReturnsAsync(orders);
            _orderRepoMock.Setup(x => x.Update(order)).ReturnsAsync(Result<Order>.Success(order));

            var restResponse = new RestResponse
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                ResponseStatus = ResponseStatus.Completed,
                IsSuccessStatusCode = true
            };

            _restClientMock.Setup(x => x.ExecuteAsync(It.IsAny<RestRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(restResponse);

            var result = await _controller.ValidateUpdateTimeDriver(token);

            var actionResult = Assert.IsType<OkResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
        }
    }
}
