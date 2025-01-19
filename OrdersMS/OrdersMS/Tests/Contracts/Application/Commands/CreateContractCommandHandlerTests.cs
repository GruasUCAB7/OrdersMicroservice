using Moq;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Utils.Optional;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Commands.CreateContract;
using OrdersMS.src.Contracts.Application.Commands.CreateContract.Types;
using OrdersMS.src.Contracts.Application.Exceptions;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain;
using OrdersMS.src.Contracts.Domain.Entities;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using Xunit;

namespace OrdersMS.Tests.Contracts.Application.Commands
{
    public class CreateContractCommandHandlerTests
    {
        private readonly Mock<IContractRepository> _contractRepositoryMock;
        private readonly Mock<IInsuredVehicleRepository> _vehicleRepositoryMock;
        private readonly Mock<IPolicyRepository> _policyRepositoryMock;
        private readonly Mock<IdGenerator<string>> _idGeneratorMock;
        private readonly CreateContractCommandHandler _handler;

        public CreateContractCommandHandlerTests()
        {
            _contractRepositoryMock = new Mock<IContractRepository>();
            _vehicleRepositoryMock = new Mock<IInsuredVehicleRepository>();
            _policyRepositoryMock = new Mock<IPolicyRepository>();
            _idGeneratorMock = new Mock<IdGenerator<string>>();
            _handler = new CreateContractCommandHandler(_contractRepositoryMock.Object, _vehicleRepositoryMock.Object, _policyRepositoryMock.Object, _idGeneratorMock.Object);
        }

        [Fact]
        public async Task ShouldCreateContractSuccess()
        {
            var command = new CreateContractCommand("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d", "53c0d8fa-dbca-4d98-9fdf-1d1413e90f5t");

            var policy = new InsurancePolicy(
                new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new PolicyType("Diamante"),
                new PolicyCoverageKm(25),
                new PolicyIncidentCoverageAmount(50),
                new PriceExtraKm(3)
            );

            var vehicle = new InsuredVehicle(
                new VehicleId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5t"),
                new VehicleBrand("Toyota"),
                new VehicleModel("Hilux"),
                new VehiclePlate("AC123CD"),
                new VehicleSize("Mediano"),
                new VehicleYear(2012),
                new ClientDNI("29611513"),
                new ClientName("Pedro Perez"),
                new ClientEmail("pedro@gmail.com")
            );

            _contractRepositoryMock.Setup(x => x.ContractExists(command.AssociatedPolicy, command.InsuredVehicle)).ReturnsAsync(false);
            _policyRepositoryMock.Setup(x => x.GetById(command.AssociatedPolicy)).ReturnsAsync(Optional<InsurancePolicy>.Of(policy));
            _policyRepositoryMock.Setup(x => x.IsActivePolicy(command.AssociatedPolicy)).ReturnsAsync(true);
            _vehicleRepositoryMock.Setup(x => x.GetById(command.InsuredVehicle)).ReturnsAsync(Optional<InsuredVehicle>.Of(vehicle));
            _vehicleRepositoryMock.Setup(x => x.IsActiveVehicle(command.InsuredVehicle)).ReturnsAsync(true);
            _idGeneratorMock.Setup(x => x.Generate()).Returns("53c0d8fa-dbca-4d98-9fdf-1d1413e90f7f");
            _contractRepositoryMock.Setup(x => x.IsContractNumberExists(6235)).ReturnsAsync(false);

            var result = await _handler.Execute(command);

            var contract = Contract.CreateContract(
                new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f7f"),
                new ContractNumber(6235),
                new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new VehicleId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5t"),
                DateTime.UtcNow
            );

            _contractRepositoryMock.Setup(x => x.Save(contract)).ReturnsAsync(Result<Contract>.Success(contract));

            Assert.NotNull(result);
            Assert.True(result.IsSuccessful);
        }

        [Fact]
        public async Task ShouldFailToCreateContract_WhenContractAlreadyExists()
        {
            var command = new CreateContractCommand("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d", "53c0d8fa-dbca-4d98-9fdf-1d1413e90f5t");

            _contractRepositoryMock.Setup(x => x.ContractExists(command.AssociatedPolicy, command.InsuredVehicle)).ReturnsAsync(true);

            var exception = await Assert.ThrowsAsync<ContractAlreadyExistException>(() => _handler.Execute(command));

            Assert.Equal($"Contract already exist with this PolicyId and VehicleId: {command.AssociatedPolicy}, {command.InsuredVehicle}.", exception.Message);
        }

        [Fact]
        public async Task ShouldFailToCreateContract_WhenNoPolicyFound()
        {
            var command = new CreateContractCommand("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d", "53c0d8fa-dbca-4d98-9fdf-1d1413e90f5t");

            _contractRepositoryMock.Setup(x => x.ContractExists(command.AssociatedPolicy, command.InsuredVehicle)).ReturnsAsync(false);
            _policyRepositoryMock.Setup(x => x.GetById(command.AssociatedPolicy)).ReturnsAsync(Optional<InsurancePolicy>.Empty());

            var exception = await Assert.ThrowsAsync<PolicyNotFoundException>(() => _handler.Execute(command));

            Assert.Equal("Policy not found", exception.Message);
        }

        [Fact]
        public async Task ShouldFailToCreateContract_WhenPolicyIsNotAvailable()
        {
            var command = new CreateContractCommand("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d", "53c0d8fa-dbca-4d98-9fdf-1d1413e90f5t");

            var policy = new InsurancePolicy(
                new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new PolicyType("Diamante"),
                new PolicyCoverageKm(25),
                new PolicyIncidentCoverageAmount(50),
                new PriceExtraKm(3)
            );

            _contractRepositoryMock.Setup(x => x.ContractExists(command.AssociatedPolicy, command.InsuredVehicle)).ReturnsAsync(false);
            _policyRepositoryMock.Setup(x => x.GetById(command.AssociatedPolicy)).ReturnsAsync(Optional<InsurancePolicy>.Of(policy));
            _policyRepositoryMock.Setup(x => x.IsActivePolicy(command.AssociatedPolicy)).ReturnsAsync(false);

            var exception = await Assert.ThrowsAsync<PolicyNotAvailableException>(() => _handler.Execute(command));

            Assert.Equal("Policy is not active", exception.Message);
        }

        [Fact]
        public async Task ShouldFailToCreateContract_WhenNoVehicleFound()
        {
            var command = new CreateContractCommand("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d", "53c0d8fa-dbca-4d98-9fdf-1d1413e90f5t");

            var policy = new InsurancePolicy(
                new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new PolicyType("Diamante"),
                new PolicyCoverageKm(25),
                new PolicyIncidentCoverageAmount(50),
                new PriceExtraKm(3)
            );

            _contractRepositoryMock.Setup(x => x.ContractExists(command.AssociatedPolicy, command.InsuredVehicle)).ReturnsAsync(false);
            _policyRepositoryMock.Setup(x => x.GetById(command.AssociatedPolicy)).ReturnsAsync(Optional<InsurancePolicy>.Of(policy));
            _policyRepositoryMock.Setup(x => x.IsActivePolicy(command.AssociatedPolicy)).ReturnsAsync(true);
            _vehicleRepositoryMock.Setup(x => x.GetById(command.InsuredVehicle)).ReturnsAsync(Optional<InsuredVehicle>.Empty());


            var exception = await Assert.ThrowsAsync<VehicleNotFoundException>(() => _handler.Execute(command));

            Assert.Equal("Vehicle not found", exception.Message);
        }

        [Fact]
        public async Task ShouldFailToCreateContract_WhenVehicleIsNotAvailable()
        {
            var command = new CreateContractCommand("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d", "53c0d8fa-dbca-4d98-9fdf-1d1413e90f5t");

            var policy = new InsurancePolicy(
                new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                new PolicyType("Diamante"),
                new PolicyCoverageKm(25),
                new PolicyIncidentCoverageAmount(50),
                new PriceExtraKm(3)
            );

            var vehicle = new InsuredVehicle(
                new VehicleId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5t"),
                new VehicleBrand("Toyota"),
                new VehicleModel("Hilux"),
                new VehiclePlate("AC123CD"),
                new VehicleSize("Mediano"),
                new VehicleYear(2012),
                new ClientDNI("29611513"),
                new ClientName("Pedro Perez"),
                new ClientEmail("pedro@gmail.com")
            );

            _contractRepositoryMock.Setup(x => x.ContractExists(command.AssociatedPolicy, command.InsuredVehicle)).ReturnsAsync(false);
            _policyRepositoryMock.Setup(x => x.GetById(command.AssociatedPolicy)).ReturnsAsync(Optional<InsurancePolicy>.Of(policy));
            _policyRepositoryMock.Setup(x => x.IsActivePolicy(command.AssociatedPolicy)).ReturnsAsync(true);
            _vehicleRepositoryMock.Setup(x => x.GetById(command.InsuredVehicle)).ReturnsAsync(Optional<InsuredVehicle>.Of(vehicle));
            _vehicleRepositoryMock.Setup(x => x.IsActiveVehicle(command.InsuredVehicle)).ReturnsAsync(false);

            var exception = await Assert.ThrowsAsync<VehicleNotAvailableException>(() => _handler.Execute(command));

            Assert.Equal("Vehicle not available", exception.Message);
        }
    }
}