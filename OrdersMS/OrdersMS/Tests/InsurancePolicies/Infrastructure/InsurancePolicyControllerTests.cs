using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Application.Logger;
using OrdersMS.Core.Utils.Optional;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Commands.CreateInsurancePolicy.Types;
using OrdersMS.src.Contracts.Application.Commands.UpdateInsuredPolicy.Types;
using OrdersMS.src.Contracts.Application.Queries.GetAllPolicies.Types;
using OrdersMS.src.Contracts.Application.Queries.GetPolicyById.Types;
using OrdersMS.src.Contracts.Application.Queries.GetVehicleById.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Application.Types;
using OrdersMS.src.Contracts.Domain.Entities;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Contracts.Infrastructure.Controllers;
using Xunit;

namespace OrdersMS.Tests.InsurancePolicies.Infrastructure
{
    public class InsurancePolicyControllerTests
    {
        private readonly Mock<IPolicyRepository> _policyRepoMock = new Mock<IPolicyRepository>();
        private readonly Mock<IdGenerator<string>> _idGeneratorMock = new Mock<IdGenerator<string>>();
        private readonly Mock<IValidator<CreatePolicyCommand>> _validatorCreateMock = new Mock<IValidator<CreatePolicyCommand>>();
        private readonly Mock<IValidator<UpdatePolicyCommand>> _validatorUpdateMock = new Mock<IValidator<UpdatePolicyCommand>>();
        private readonly Mock<ILoggerContract> _loggerMock = new Mock<ILoggerContract>();
        private readonly InsurancePolicyController _controller;

        public InsurancePolicyControllerTests()
        {
            _controller = new InsurancePolicyController(_policyRepoMock.Object, _idGeneratorMock.Object, _validatorCreateMock.Object, _validatorUpdateMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateInsurancePolicy_ShouldReturnCreated()
        {
            var command = new CreatePolicyCommand("Diamante", 25, 50, 3);
            var craneId = new CreatePolicyResponse("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");

            _idGeneratorMock.Setup(x => x.Generate()).Returns(craneId.Id);

            _validatorCreateMock.Setup(x => x.Validate(command)).Returns(new ValidationResult());

            var result = await _controller.CreatePolicy(command);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, actionResult.StatusCode);
        }

        [Fact]
        public async Task CreateInsurancePolicy_ShouldReturn400_WhenValidationFails()
        {
            var command = new CreatePolicyCommand("Diamante", 25, 50, 3);
            var validationResult = new ValidationResult(new List<ValidationFailure> { new ValidationFailure("Type", "Type is required") });
            _validatorCreateMock.Setup(x => x.Validate(command)).Returns(validationResult);

            var result = await _controller.CreatePolicy(command) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result?.StatusCode);
            Assert.Equal(new List<string> { "Type is required" }, result?.Value);
        }

        [Fact]
        public async Task CreateInsurancePolicy_ShouldReturn409_WhenPolicyAlreadyExists()
        {
            var command = new CreatePolicyCommand("Diamante", 25, 50, 3);
            var craneId = new CreatePolicyResponse("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");
            _validatorCreateMock.Setup(x => x.Validate(command)).Returns(new ValidationResult());
            _policyRepoMock.Setup(x => x.ExistByType(command.Type)).ReturnsAsync(true);

            var result = await _controller.CreatePolicy(command);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, actionResult.StatusCode);
        }

        [Fact]
        public async Task GetAllInsurancePolicies_ShouldReturn200_WhenPoliciesAreRetrievedSuccessfully()
        {
            var query = new GetAllPoliciesQuery(10, 1, "active");
            var policies = new List<InsurancePolicy>
            {
                    new InsurancePolicy(
                        new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f2h"),
                        new PolicyType("Diamante"),
                        new PolicyCoverageKm(30),
                        new PolicyIncidentCoverageAmount(55),
                        new PriceExtraKm(2)
                    ),
                    new InsurancePolicy(
                        new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90ffr"),
                        new PolicyType("Premium"),
                        new PolicyCoverageKm(40),
                        new PolicyIncidentCoverageAmount(80),
                        new PriceExtraKm(3)
                    )
            };

            var policyResponses = policies.Select(p => new GetPolicyResponse(p.GetId(), p.GetPolicyType(), p.GetPolicyCoverageKm(), p.GetPolicyIncidentCoverageAmount(), p.GetPriceExtraKm(), p.GetIsActive())).ToArray();

            _policyRepoMock.Setup(x => x.GetAll(query)).ReturnsAsync(policies);

            var result = await _controller.GetAllPolicies(query);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);

            var responseValue = Assert.IsType<GetPolicyResponse[]>(actionResult.Value);
            Assert.Equal(policyResponses, responseValue);
        }

        [Fact]
        public async Task GetInsurancePolicyById_ShouldReturn200_WhenPolicyExist()
        {
            var policyId = new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");
            var type = new PolicyType("Diamante");
            var coverageKm = new PolicyCoverageKm(30);
            var coverageAmount = new PolicyIncidentCoverageAmount(55);
            var priceExtraKm = new PriceExtraKm(2);
            var existingPolicy = new InsurancePolicy(policyId, type, coverageKm, coverageAmount, priceExtraKm);

            var query = new GetVehicleByIdQuery(policyId.GetValue());

            _policyRepoMock.Setup(r => r.GetById(policyId.GetValue())).ReturnsAsync(Optional<InsurancePolicy>.Of(existingPolicy));

            var result = await _controller.GetPolicyById(policyId.GetValue());

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
        }

        [Fact]
        public async Task GetInsurancePolicyById_ShouldReturn500_WhenPolicyNotFound()
        {
            var query = new GetPolicyByIdQuery("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");

            _policyRepoMock.Setup(r => r.GetById("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e")).ReturnsAsync(Optional<InsurancePolicy>.Empty());

            var result = await _controller.GetPolicyById(query.Id);

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, actionResult.StatusCode);
        }

        [Fact]
        public async Task UpdateInsurancePolicy_ShouldReturn200_WhenUpdateIsSuccessful()
        {
            var policyId = new PolicyId("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");
            var type = new PolicyType("Diamante");
            var coverageKm = new PolicyCoverageKm(30);
            var coverageAmount = new PolicyIncidentCoverageAmount(55);
            var priceExtraKm = new PriceExtraKm(2);
            var existingPolicy = new InsurancePolicy(policyId, type, coverageKm, coverageAmount, priceExtraKm);

            var command = new UpdatePolicyCommand(true, 30, 55, 2);

            _validatorUpdateMock.Setup(x => x.Validate(command)).Returns(new ValidationResult());

            _policyRepoMock.Setup(r => r.GetById(policyId.GetValue())).ReturnsAsync(Optional<InsurancePolicy>.Of(existingPolicy));
            _policyRepoMock.Setup(r => r.Update(existingPolicy)).ReturnsAsync(Result<InsurancePolicy>.Success(existingPolicy));

            var result = await _controller.UpdatePolicy(command, policyId.GetValue());

            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
        }

        [Fact]
        public async Task UpdateInsurancePolicy_ShouldReturn400_WhenValidationFails()
        {
            var command = new UpdatePolicyCommand(true, 30, 55, 2);
            var validationResult = new ValidationResult(new List<ValidationFailure> { new ValidationFailure("IsActive", "IsActive is required") });
            _validatorUpdateMock.Setup(x => x.Validate(command)).Returns(validationResult);

            var result = await _controller.UpdatePolicy(command, "53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e") as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result?.StatusCode);
            Assert.Equal(new List<string> { "IsActive is required" }, result?.Value);
        }

        [Fact]
        public async Task UpdateInsurancePolicy_ShouldReturn409_WhenPolicyNotFound()
        {
            var command = new UpdatePolicyCommand(true, 30, 55, 2);

            _validatorUpdateMock.Setup(x => x.Validate(command)).Returns(new ValidationResult());
            _policyRepoMock.Setup(r => r.GetById("53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e")).ReturnsAsync(Optional<InsurancePolicy>.Empty());

            var result = await _controller.UpdatePolicy(command, "53c0d8fa-dbca-4d98-9fdf-1d1413e90f0e");

            var actionResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(409, actionResult.StatusCode);
        }
    }
}
