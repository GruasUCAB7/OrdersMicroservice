using FluentValidation;
using FluentValidation.Results;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.src.Contracts.Application.Commands.CreateInsuredVehicle.Types;
using OrdersMS.src.Contracts.Application.Commands.UpdateInsuredVehicle.Types;
using OrdersMS.Core.Application.Logger;
using OrdersMS.src.Contracts.Infrastructure.Controllers;
using OrdersMS.src.Contracts.Application.Queries.GetAllVehicles.Types;
using OrdersMS.src.Contracts.Domain.Entities;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Contracts.Application.Types;
using OrdersMS.src.Contracts.Application.Queries.GetVehicleById.Types;
using OrdersMS.Core.Utils.Optional;
using OrdersMS.Core.Utils.Result;

namespace OrdersMS.Tests.InsuredVehicles.Infrastructure
{
    public class InsuredVehicleControllerTests
    {
        private readonly Mock<IInsuredVehicleRepository> _vehicleRepoMock = new Mock<IInsuredVehicleRepository>();
        private readonly Mock<IdGenerator<string>> _idGeneratorMock = new Mock<IdGenerator<string>>();
        private readonly Mock<IValidator<CreateVehicleCommand>> _validatorCreateMock = new Mock<IValidator<CreateVehicleCommand>>();
        private readonly Mock<IValidator<UpdateVehicleCommand>> _validatorUpdateMock = new Mock<IValidator<UpdateVehicleCommand>>();
        private readonly Mock<ILoggerContract> _loggerMock = new Mock<ILoggerContract>();
        private readonly InsuredVehicleController _controller;

        public InsuredVehicleControllerTests()
        {
            _controller = new InsuredVehicleController(_vehicleRepoMock.Object, _idGeneratorMock.Object, _validatorCreateMock.Object, _validatorUpdateMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateInsuredVehicle_ShouldReturnCreated()
        {
            var command = new CreateVehicleCommand("Toyota", "Hilux", "AC123CD", "Mediano", 2012, "29611513", "Pedro Perez", "pedro@gmail.com");
            var craneId = new CreateVehicleResponse("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");

            _idGeneratorMock.Setup(x => x.Generate()).Returns(craneId.Id);

            _validatorCreateMock.Setup(x => x.Validate(command)).Returns(new ValidationResult());

            var result = await _controller.CreateVehicle(command);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, actionResult.StatusCode);
        }

        [Fact]
        public async Task CreateInsuredVehicle_ShouldReturn400_WhenValidationFails()
        {
            var command = new CreateVehicleCommand("", "Hilux", "AC123CD", "Mediano", 2012, "29611513", "Pedro Perez", "pedro@gmail.com");
            var validationResult = new ValidationResult(new List<ValidationFailure> { new ValidationFailure("Brand", "Brand is required") });
            _validatorCreateMock.Setup(x => x.Validate(command)).Returns(validationResult);

            var result = await _controller.CreateVehicle(command) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result?.StatusCode);
            Assert.Equal(new List<string> { "Brand is required" }, result?.Value);
        }

        [Fact]
        public async Task CreateInsuredVehicle_ShouldReturn409_WhenVehicleAlreadyExists()
        {
            var command = new CreateVehicleCommand("Toyota", "Hilux", "AC123CD", "Mediano", 2012, "29611513", "Pedro Perez", "pedro@gmail.com");
            var craneId = new CreateVehicleResponse("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");
            _validatorCreateMock.Setup(x => x.Validate(command)).Returns(new ValidationResult());
            _vehicleRepoMock.Setup(x => x.ExistByPlate(command.Plate)).ReturnsAsync(true);

            var result = await _controller.CreateVehicle(command);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, actionResult.StatusCode);
        }

        [Fact]
        public async Task GetAllInsuredVehicles_ShouldReturn200_WhenVehiclesAreRetrievedSuccessfully()
        {
            var query = new GetAllVehiclesQuery(10, 1, "active");
            var vehicles = new List<InsuredVehicle>
            {
                new InsuredVehicle(
                        new VehicleId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                        new VehicleBrand("Toyota"),
                        new VehicleModel("Hilux"),
                        new VehiclePlate("AC123CD"),
                        new VehicleSize("Mediano"),
                        new VehicleYear(2012),
                        new ClientDNI("29611513"),
                        new ClientName("Pedro Perez"),
                        new ClientEmail("pedro@gmail.com")
                    ),
                    new InsuredVehicle(
                        new VehicleId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f1g"),
                        new VehicleBrand("Toyota"),
                        new VehicleModel("Hilux"),
                        new VehiclePlate("AC524RT"),
                        new VehicleSize("Pesado"),
                        new VehicleYear(2010),
                        new ClientDNI("29611514"),
                        new ClientName("Juan Gomez"),
                        new ClientEmail("juan@gmail.com")
                    )
            };

            var vehicleResponses = vehicles.Select(v => new GetVehicleResponse(v.GetId(), v.GetBrand(), v.GetModel(), v.GetPlate(), v.GetVehicleSize(), v.GetYear(), v.GetIsActive(), v.GetClientDNI(), v.GetClientName(), v.GetClientEmail())).ToArray();

            _vehicleRepoMock.Setup(x => x.GetAll(query)).ReturnsAsync(vehicles);

            var result = await _controller.GetAllVehicles(query);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);

            var responseValue = Assert.IsType<GetVehicleResponse[]>(actionResult.Value);
            Assert.Equal(vehicleResponses, responseValue);
        }

        [Fact]
        public async Task GetVehicleById_ShouldReturn200_WhenVehicleExist()
        {
            var vehicleId = new VehicleId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");
            var brand = new VehicleBrand("Ford");
            var model = new VehicleModel("Tritón");
            var plate = new VehiclePlate("AB123CD");
            var size = new VehicleSize("Pesado");
            var year = new VehicleYear(2012);
            var clientDNI = new ClientDNI("29611513");
            var clientName = new ClientName("Pedro Perez");
            var clientEmail = new ClientEmail("pedro@mgial.com");
            var existingVehicle = new InsuredVehicle(vehicleId, brand, model, plate, size, year, clientDNI, clientName, clientEmail);

            var query = new GetVehicleByIdQuery(vehicleId.GetValue());

            _vehicleRepoMock.Setup(r => r.GetById(vehicleId.GetValue())).ReturnsAsync(Optional<InsuredVehicle>.Of(existingVehicle));

            var result = await _controller.GetVehicleById(vehicleId.GetValue());

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
        }

        [Fact]
        public async Task GetVehicleById_ShouldReturn500_WhenVehicleNotFound()
        {
            var query = new GetVehicleByIdQuery("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");

            _vehicleRepoMock.Setup(r => r.GetById("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e")).ReturnsAsync(Optional<InsuredVehicle>.Empty());

            var result = await _controller.GetVehicleById(query.Id);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, actionResult.StatusCode);
        }

        [Fact]
        public async Task UpdateVehicle_ShouldReturn200_WhenUpdateIsSuccessful()
        {
            var vehicleId = new VehicleId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");
            var brand = new VehicleBrand("Ford");
            var model = new VehicleModel("Tritón");
            var plate = new VehiclePlate("AB123CD");
            var size = new VehicleSize("Pesado");
            var year = new VehicleYear(2012);
            var clientDNI = new ClientDNI("29611513");
            var clientName = new ClientName("Pedro Perez");
            var clientEmail = new ClientEmail("pedro@mgial.com");
            var existingVehicle = new InsuredVehicle(vehicleId, brand, model, plate, size, year, clientDNI, clientName, clientEmail);

            var command = new UpdateVehicleCommand(true);

            _validatorUpdateMock.Setup(x => x.Validate(command)).Returns(new ValidationResult());

            _vehicleRepoMock.Setup(r => r.GetById(vehicleId.GetValue())).ReturnsAsync(Optional<InsuredVehicle>.Of(existingVehicle));
            _vehicleRepoMock.Setup(r => r.Update(existingVehicle)).ReturnsAsync(Result<InsuredVehicle>.Success(existingVehicle));

            var result = await _controller.UpdateVehicle(command, vehicleId.GetValue());

            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
        }

        [Fact]
        public async Task UpdateVehicle_ShouldReturn400_WhenValidationFails()
        {
            var command = new UpdateVehicleCommand(true);
            var validationResult = new ValidationResult(new List<ValidationFailure> { new ValidationFailure("IsActive", "IsActive is required") });
            _validatorUpdateMock.Setup(x => x.Validate(command)).Returns(validationResult);

            var result = await _controller.UpdateVehicle(command, "53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e") as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result?.StatusCode);
            Assert.Equal(new List<string> { "IsActive is required" }, result?.Value);
        }

        [Fact]
        public async Task UpdateVehicle_ShouldReturn409_WhenVehicleNotFound()
        {
            var command = new UpdateVehicleCommand(true);

            _validatorUpdateMock.Setup(x => x.Validate(command)).Returns(new ValidationResult());
            _vehicleRepoMock.Setup(r => r.GetById("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e")).ReturnsAsync(Optional<InsuredVehicle>.Empty());

            var result = await _controller.UpdateVehicle(command, "53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, actionResult.StatusCode);
        }
    }
}
