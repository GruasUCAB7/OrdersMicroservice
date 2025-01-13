using Moq;
using OrdersMS.Core.Utils.Optional;
using OrdersMS.src.Contracts.Application.Queries.GetVehicleById;
using OrdersMS.src.Contracts.Application.Queries.GetVehicleById.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain.Entities;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using Xunit;

namespace OrdersMS.Tests.InsuredVehicles.Application.Queries
{
    public class GetVehicleByIdQueryHandlerTests
    {
        private readonly Mock<IInsuredVehicleRepository> _vehicleRepositoryMock;

        public GetVehicleByIdQueryHandlerTests()
        {
            _vehicleRepositoryMock = new Mock<IInsuredVehicleRepository>();
        }

        [Fact]
        public async Task GetInsuredVehicleByIdSuccess()
        {
            var query = new GetVehicleByIdQuery("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");
            var handler = new GetVehicleByIdQueryHandler(_vehicleRepositoryMock.Object);
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

            _vehicleRepositoryMock.Setup(x => x.GetById(query.Id)).ReturnsAsync(Optional<InsuredVehicle>.Of(vehicle));

            var result = await handler.Execute(query);

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public async Task GetInsuredVehicleByIdWhenCraneNotFound()
        {
            var query = new GetVehicleByIdQuery("53c0d8fa-dbca-4d98-9fdf-1d1413e90f9f");
            var handler = new GetVehicleByIdQueryHandler(_vehicleRepositoryMock.Object);

            _vehicleRepositoryMock.Setup(x => x.GetById(query.Id)).ReturnsAsync(Optional<InsuredVehicle>.Empty());

            var result = await handler.Execute(query);

            Assert.NotNull(result);
            Assert.False(result.IsSuccessful);
            Assert.Equal("Vehicle not found", result.ErrorMessage);
        }
    }
}
