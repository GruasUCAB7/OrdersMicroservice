using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Application.Logger;
using OrdersMS.Core.Utils.Optional;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Commands.CreateContract.Types;
using OrdersMS.src.Contracts.Application.Commands.UpdateContract.Types;
using OrdersMS.src.Contracts.Application.Queries.GetAllContracts.Types;
using OrdersMS.src.Contracts.Application.Queries.GetContractById.Types;
using OrdersMS.src.Contracts.Application.Queries.GetContractId.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Application.Types;
using OrdersMS.src.Contracts.Domain;
using OrdersMS.src.Contracts.Domain.Entities;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Contracts.Infrastructure.Controllers;
using Xunit;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace OrdersMS.Tests.Contracts.Infrastructure
{
    public class ContractControllerTests
    {
        private readonly Mock<IContractRepository> _contractRepoMock = new Mock<IContractRepository>();
        private readonly Mock<IPolicyRepository> _policyRepoMock = new Mock<IPolicyRepository>();
        private readonly Mock<IInsuredVehicleRepository> _vehicleRepoMock = new Mock<IInsuredVehicleRepository>();
        private readonly Mock<IdGenerator<string>> _idGeneratorMock = new Mock<IdGenerator<string>>();
        private readonly Mock<IValidator<CreateContractCommand>> _validatorCreateMock = new Mock<IValidator<CreateContractCommand>>();
        private readonly Mock<IValidator<UpdateContractCommand>> _validatorUpdateMock = new Mock<IValidator<UpdateContractCommand>>();
        private readonly Mock<ILoggerContract> _loggerMock = new Mock<ILoggerContract>();
        private readonly ContractController _controller;

        public ContractControllerTests()
        {
            _controller = new ContractController(_contractRepoMock.Object, _policyRepoMock.Object, _vehicleRepoMock.Object, _idGeneratorMock.Object, _validatorCreateMock.Object, _validatorUpdateMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateContract_ShouldReturnCreated()
        {
            var command = new CreateContractCommand("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d", "53c0d8fa-dbca-4d98-9fdf-1d1413e90f5t");
            var contractId = new CreateContractResponse("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");

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

            _validatorCreateMock.Setup(x => x.Validate(command)).Returns(new ValidationResult());
            _contractRepoMock.Setup(x => x.ContractExists(command.AssociatedPolicy, command.InsuredVehicle)).ReturnsAsync(false);
            _policyRepoMock.Setup(x => x.GetById(command.AssociatedPolicy)).ReturnsAsync(Optional<InsurancePolicy>.Of(policy));
            _policyRepoMock.Setup(x => x.IsActivePolicy(command.AssociatedPolicy)).ReturnsAsync(true);
            _vehicleRepoMock.Setup(x => x.GetById(command.InsuredVehicle)).ReturnsAsync(Optional<InsuredVehicle>.Of(vehicle));
            _vehicleRepoMock.Setup(x => x.IsActiveVehicle(command.InsuredVehicle)).ReturnsAsync(true);
            _idGeneratorMock.Setup(x => x.Generate()).Returns(contractId.Id);
            _contractRepoMock.Setup(x => x.IsContractNumberExists(6235)).ReturnsAsync(false);

            var result = await _controller.CreateContract(command);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, actionResult.StatusCode);
        }

        [Fact]
        public async Task CreateContract_ShouldReturn400_WhenValidationFails()
        {
            var command = new CreateContractCommand("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d", "53c0d8fa-dbca-4d98-9fdf-1d1413e90f5t");
            var validationResult = new ValidationResult(new List<ValidationFailure> { new ValidationFailure("AssociatedPolicy", "Policy ID is required.") });
            _validatorCreateMock.Setup(x => x.Validate(command)).Returns(validationResult);

            var result = await _controller.CreateContract(command) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result?.StatusCode);
            Assert.Equal(new List<string> { "Policy ID is required." }, result?.Value);
        }

        [Fact]
        public async Task CreateContract_ShouldReturn500_WhenContractAlreadyExists()
        {
            var command = new CreateContractCommand("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d", "53c0d8fa-dbca-4d98-9fdf-1d1413e90f5t");
            var contractId = new CreateContractResponse("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");
            _validatorCreateMock.Setup(x => x.Validate(command)).Returns(new ValidationResult());
            _contractRepoMock.Setup(x => x.ContractExists(command.AssociatedPolicy, command.InsuredVehicle)).ReturnsAsync(true);

            var result = await _controller.CreateContract(command);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, actionResult.StatusCode);
        }

        [Fact]
        public async Task GetAllContracts_ShouldReturn200_WhenContractsAreRetrievedSuccessfully()
        {
            var query = new GetAllContractsQuery(10, 1, "");
            var contracts = new List<Contract>
            {
                    Contract.CreateContract(
                        new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f7f"),
                        new ContractNumber(6235),
                        new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0d"),
                        new VehicleId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5t")
                    ),
                    Contract.CreateContract(
                        new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f5e"),
                        new ContractNumber(6235),
                        new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f7h"),
                        new VehicleId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f55")
                    ),
            };

            var contractResponses = contracts.Select(c => new GetContractResponse(c.GetId(), c.GetContractNumber(), c.GetPolicyId(), c.GetVehicleId(), c.GetStartDate(), c.GetExpirationDate(), c.GetStatus())).ToArray();

            _contractRepoMock.Setup(x => x.GetAll(query)).ReturnsAsync(contracts);

            var result = await _controller.GetAllContracts(query);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);

            var responseValue = Assert.IsType<GetContractResponse[]>(actionResult.Value);
            Assert.Equal(contractResponses, responseValue);
        }

        [Fact]
        public async Task GetContractById_ShouldReturn200_WhenContractExist()
        {
            var contractId = new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");
            var number = new ContractNumber(6235);
            var policyId = new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f7h");
            var vehicleId = new VehicleId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f55");
            var existingContract = Contract.CreateContract(contractId, number, policyId, vehicleId);

            var query = new GetContractByIdQuery(contractId.GetValue());

            _contractRepoMock.Setup(r => r.GetById(contractId.GetValue())).ReturnsAsync(Optional<Contract>.Of(existingContract));

            var result = await _controller.GetContractById(contractId.GetValue());

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
        }

        [Fact]
        public async Task GetContractById_ShouldReturn500_WhenContractNotFound()
        {
            var query = new GetContractByIdQuery("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");

            _contractRepoMock.Setup(r => r.GetById("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e")).ReturnsAsync(Optional<Contract>.Empty());

            var result = await _controller.GetContractById(query.Id);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, actionResult.StatusCode);
        }

        [Fact]
        public async Task UpdateContract_ShouldReturn200_WhenUpdateIsSuccessful()
        {
            var contractId = new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");
            var number = new ContractNumber(6235);
            var policyId = new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f7h");
            var vehicleId = new VehicleId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f55");
            var existingContract = Contract.CreateContract(contractId, number, policyId, vehicleId);

            var command = new UpdateContractCommand("Activo");

            _validatorUpdateMock.Setup(x => x.Validate(command)).Returns(new ValidationResult());

            _contractRepoMock.Setup(r => r.GetById(contractId.GetValue())).ReturnsAsync(Optional<Contract>.Of(existingContract));
            _contractRepoMock.Setup(r => r.Update(existingContract)).ReturnsAsync(Result<Contract>.Success(existingContract));

            var result = await _controller.UpdateContract(command, contractId.GetValue());

            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
        }

        [Fact]
        public async Task UpdateContract_ShouldReturn400_WhenValidationFails()
        {
            var command = new UpdateContractCommand("");
            var validationResult = new ValidationResult(new List<ValidationFailure> { new ValidationFailure("Status", "Status is required") });
            _validatorUpdateMock.Setup(x => x.Validate(command)).Returns(validationResult);

            var result = await _controller.UpdateContract(command, "53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e") as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result?.StatusCode);
            Assert.Equal(new List<string> { "Status is required" }, result?.Value);
        }

        [Fact]
        public async Task UpdateContract_ShouldReturn409_WhenContractNotFound()
        {
            var command = new UpdateContractCommand("Activo");

            _validatorUpdateMock.Setup(x => x.Validate(command)).Returns(new ValidationResult());
            _contractRepoMock.Setup(r => r.GetById("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e")).ReturnsAsync(Optional<Contract>.Empty());

            var result = await _controller.UpdateContract(command, "53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, actionResult.StatusCode);
        }

        [Fact]
        public async Task GetContractIdByContractNumber_ShouldReturn200_WhenContractExist()
        {
            var contractId = new ContractId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");
            var number = new ContractNumber(6235);
            var policyId = new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f7h");
            var vehicleId = new VehicleId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f55");
            var existingContract = Contract.CreateContract(contractId, number, policyId, vehicleId);

            var query = new GetContractIdQuery(number.GetValue());

            _contractRepoMock.Setup(r => r.GetContractIdByContractNumber(number.GetValue())).ReturnsAsync("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");

            var result = await _controller.GetContractIdByContractNumber(number.GetValue());

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
        }

        [Fact]
        public async Task GetContractIdByContractNumber_ShouldReturn500_WhenContractNotFound()
        {
            var number = new ContractNumber(6235);

            var query = new GetContractIdQuery(number.GetValue());

            _contractRepoMock.Setup(r => r.GetContractIdByContractNumber(number.GetValue())).ReturnsAsync("");

            var result = await _controller.GetContractIdByContractNumber(number.GetValue());

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, actionResult.StatusCode);
        }
    }
}
