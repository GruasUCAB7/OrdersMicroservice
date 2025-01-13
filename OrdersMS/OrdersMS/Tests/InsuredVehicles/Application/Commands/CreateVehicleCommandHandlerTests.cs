using Moq;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Commands.CreateInsuredVehicle;
using OrdersMS.src.Contracts.Application.Commands.CreateInsuredVehicle.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain.Entities;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using Xunit;

namespace OrdersMS.Tests.InsuredVehicles.Application.Commands
{
    public class CreateVehicleCommandHandlerTests
    {
        private readonly Mock<IInsuredVehicleRepository> _vehicleRepositoryMock;
        private readonly Mock<IdGenerator<string>> _idGeneratorMock;
        private readonly CreateVehicleCommandHandler _handler;

        public CreateVehicleCommandHandlerTests()
        {
            _vehicleRepositoryMock = new Mock<IInsuredVehicleRepository>();
            _idGeneratorMock = new Mock<IdGenerator<string>>();
            _handler = new CreateVehicleCommandHandler(_vehicleRepositoryMock.Object, _idGeneratorMock.Object);
        }

        [Fact]
        public async Task ShouldCreateInsuredVehicleSuccess()
        {
            var command = new CreateVehicleCommand("Toyota", "Hilux", "AC123CD", "Mediano", 2012, "29611513", "Pedro Perez", "pedro@gmail.com");

            _vehicleRepositoryMock.Setup(x => x.ExistByPlate(command.Plate)).ReturnsAsync(false);
            _idGeneratorMock.Setup(x => x.Generate()).Returns("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d");
            var result = await _handler.Execute(command);

            var vehicle = new InsuredVehicle(
                new VehicleId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new VehicleBrand(command.Brand),
                new VehicleModel(command.Model),
                new VehiclePlate(command.Plate),
                new VehicleSize(command.VehicleSize),
                new VehicleYear(command.Year),
                new ClientDNI(command.ClientDNI),
                new ClientName(command.ClientName),
                new ClientEmail(command.ClientEmail)
            );

            _vehicleRepositoryMock.Setup(x => x.Save(vehicle)).ReturnsAsync(Result<InsuredVehicle>.Success(vehicle));

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public async Task ShouldFailToCreateInsuredVehicle_WhenInsuredVehicleAlreadyExists()
        {
            var command = new CreateVehicleCommand("Toyota", "Hilux", "AC123CD", "Mediano", 2012, "29611513", "Pedro Perez", "pedro@gmail.com");

            _vehicleRepositoryMock.Setup(x => x.ExistByPlate(command.Plate)).ReturnsAsync(true);
            var result = await _handler.Execute(command);

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal($"Vehicle with plate AC123CD already exist", result.ErrorMessage);
        }
    }
}