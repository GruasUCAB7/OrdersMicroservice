using Xunit;
using Moq;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Application.Queries.GetAllVehicles.Types;
using OrdersMS.src.Contracts.Application.Queries.GetAllVehicles;
using OrdersMS.src.Contracts.Domain.Entities;
using OrdersMS.src.Contracts.Domain.ValueObjects;


namespace OrdersMS.Tests.InsuredVehicles.Application.Queries
{
    public class GetAllVehiclesQueryHandlerTests
    {
        private readonly Mock<IInsuredVehicleRepository> _vehicleRepositoryMock;

        public GetAllVehiclesQueryHandlerTests()
        {
            _vehicleRepositoryMock = new Mock<IInsuredVehicleRepository>();
        }

        [Fact]
        public async Task GetAllCranesSuccess()
        {
            var query = new GetAllVehiclesQuery(1, 5, "");
            var handler = new GetAllVehiclesQueryHandler(_vehicleRepositoryMock.Object);
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


            _vehicleRepositoryMock.Setup(x => x.GetAll(query)).ReturnsAsync(vehicles);
            var result = await handler.Execute(query);

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.Equal(2, result.Unwrap().Length);
        }

        [Fact]
        public async Task GetAllInsuredVehiclesFailureWhenNoVehiclesFound()
        {
            var query = new GetAllVehiclesQuery(1, 5, "");
            var handler = new GetAllVehiclesQueryHandler(_vehicleRepositoryMock.Object);

            _vehicleRepositoryMock.Setup(x => x.GetAll(query)).ReturnsAsync(new List<InsuredVehicle>());

            var result = await handler.Execute(query);

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
            Assert.Empty(result.Unwrap());
        }
    }
}
