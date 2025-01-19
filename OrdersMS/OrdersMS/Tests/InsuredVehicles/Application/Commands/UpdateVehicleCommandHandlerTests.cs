using Moq;
using OrdersMS.Core.Utils.Optional;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Commands.UpdateInsuredVehicle;
using OrdersMS.src.Contracts.Application.Commands.UpdateInsuredVehicle.Types;
using OrdersMS.src.Contracts.Application.Exceptions;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain.Entities;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using Xunit;

namespace OrdersMS.Tests.InsuredVehicles.Application.Commands
{
    public class UpdateVehicleCommandHandlerTests
    {
        private readonly Mock<IInsuredVehicleRepository> _vehicleRepositoryMock;

        public UpdateVehicleCommandHandlerTests()
        {
            _vehicleRepositoryMock = new Mock<IInsuredVehicleRepository>();
        }

        [Fact]
        public async Task ShouldUpdateInsuredVehicleSucces()
        {
            var vehicleId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d";
            var command = new UpdateVehicleCommand(true);
            var handler = new UpdateVehicleCommandHandler(_vehicleRepositoryMock.Object);

            var vehicle = new InsuredVehicle(
                new VehicleId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new VehicleBrand("Toyota"),
                new VehicleModel("Hilux"),
                new VehiclePlate("AC123CD"),
                new VehicleSize("Mediano"),
                new VehicleYear(2012),
                new ClientDNI("29611513"),
                new ClientName("Pedro Perez"),
                new ClientEmail("pedro@gmail.com")
            );

            _vehicleRepositoryMock.Setup(x => x.GetById(vehicleId)).ReturnsAsync(Optional<InsuredVehicle>.Of(vehicle));
            _vehicleRepositoryMock.Setup(x => x.Update(vehicle)).ReturnsAsync(Result<InsuredVehicle>.Success(vehicle));

            var result = await handler.Execute((vehicleId, command));
            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.Equal(vehicleId, result.Unwrap().Id);
            Assert.Equal("Toyota", result.Unwrap().Brand);
            Assert.Equal("Hilux", result.Unwrap().Model);
            Assert.Equal("AC123CD", result.Unwrap().Plate);
            Assert.Equal("Mediano", result.Unwrap().VehicleSize);
            Assert.Equal(2012, result.Unwrap().Year);
            Assert.Equal("29611513", result.Unwrap().ClientDNI);
            Assert.Equal("Pedro Perez", result.Unwrap().ClientName);
            Assert.Equal("pedro@gmail.com", result.Unwrap().ClientEmail);
            Assert.True(result.Unwrap().IsActive);
        }

        [Fact]
        public async Task ShouldFailToUpdateInsuredVehicleWhenCraneNotFound()
        {
            var vehicleId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d";
            var command = new UpdateVehicleCommand(true);
            var handler = new UpdateVehicleCommandHandler(_vehicleRepositoryMock.Object);

            _vehicleRepositoryMock.Setup(x => x.GetById(vehicleId)).ReturnsAsync(Optional<InsuredVehicle>.Empty());

            var result = await handler.Execute((vehicleId, command));

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("Vehicle not found", result.ErrorMessage);
        }

        [Fact]
        public async Task ShouldFailToUpdateInsuredVehicleWhenUpdateFails()
        {
            var vehicleId = "53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d";
            var command = new UpdateVehicleCommand(true);
            var handler = new UpdateVehicleCommandHandler(_vehicleRepositoryMock.Object);

            var vehicle = new InsuredVehicle(
                new VehicleId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new VehicleBrand("Toyota"),
                new VehicleModel("Hilux"),
                new VehiclePlate("AC123CD"),
                new VehicleSize("Mediano"),
                new VehicleYear(2012),
                new ClientDNI("29611513"),
                new ClientName("Pedro Perez"),
                new ClientEmail("pedro@gmail.com")
            );

            _vehicleRepositoryMock.Setup(x => x.GetById(vehicleId)).ReturnsAsync(Optional<InsuredVehicle>.Of(vehicle));
            _vehicleRepositoryMock.Setup(x => x.Update(vehicle)).ReturnsAsync(Result<InsuredVehicle>.Failure(new VehicleUpdateFailedException()));

            var result = await handler.Execute((vehicleId, command));

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("The vehicle could not be updated correctly", result.ErrorMessage);
        }
    }
}
