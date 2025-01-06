using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Application.Logger;
using OrdersMS.src.Contracts.Application.Commands.CreateInsurancePolicy;
using OrdersMS.src.Contracts.Application.Commands.CreateInsurancePolicy.Types;
using OrdersMS.src.Contracts.Application.Commands.UpdateInsuredPolicy;
using OrdersMS.src.Contracts.Application.Commands.UpdateInsuredPolicy.Types;
using OrdersMS.src.Contracts.Application.Queries.GetAllPolicies;
using OrdersMS.src.Contracts.Application.Queries.GetAllPolicies.Types;
using OrdersMS.src.Contracts.Application.Queries.GetPolicyById;
using OrdersMS.src.Contracts.Application.Queries.GetPolicyById.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Application.Types;

namespace OrdersMS.src.Contracts.Infrastructure.Controllers
{
    [Route("order/policy")]
    [ApiController]
    public class InsurancePolicyController(
        IPolicyRepository policyRepo,
        IdGenerator<string> idGenerator,
        IValidator<CreatePolicyCommand> validatorCreate,
        IValidator<UpdatePolicyCommand> validatorUpdate,
        ILoggerContract logger) : ControllerBase
    {
        private readonly IPolicyRepository _policyRepo = policyRepo;
        private readonly IdGenerator<string> _idGenerator = idGenerator;
        private readonly IValidator<CreatePolicyCommand> _validatorCreate = validatorCreate;
        private readonly IValidator<UpdatePolicyCommand> _validatorUpdate = validatorUpdate;
        private readonly ILoggerContract _logger = logger;

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePolicy([FromBody] CreatePolicyCommand data)
        {
            try
            {
                var command = new CreatePolicyCommand(data.Type, data.CoverageKm, data.CoverageAmount, data.PriceExtraKm);

                var validate = _validatorCreate.Validate(command);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.Error($"Validation failed for CreatePolicyCommand: {string.Join(", ", errors)}");
                    return StatusCode(400, errors);
                }

                var handler = new CreatePolicyCommandHandler(_policyRepo, _idGenerator);
                var result = await handler.Execute(command);

                if (result.IsSuccessful)
                {
                    _logger.Log("Policy created successfully: {PolicyId}", result.Unwrap().Id);
                    return StatusCode(201, new { id = result.Unwrap().Id });
                }
                else
                {
                    _logger.Error("Failed to create policy: {ErrorMessage}", result.ErrorMessage);
                    return StatusCode(409, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.Exception("An error occurred while creating the insurance policy.", ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPolicies([FromQuery] GetAllPoliciesQuery data)
        {
            try
            {
                var query = new GetAllPoliciesQuery(data.PerPage, data.Page, data.IsActive);
                var handler = new GetAllPoliciesQueryHandler(_policyRepo);
                var result = await handler.Execute(query);

                _logger.Log("List of policies: {PolicyIds}", string.Join(", ", result.Unwrap().Select(c => c.Id)));
                return StatusCode(200, result.Unwrap());
            }
            catch (Exception ex)
            {
                _logger.Exception("Failed to get list of policies", ex.Message);
                return StatusCode(200, Array.Empty<GetPolicyResponse>());
            }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, Operator")]
        public async Task<IActionResult> GetPolicyById(string id)
        {
            try
            {
                var query = new GetPolicyByIdQuery(id);
                var handler = new GetPolicyByIdQueryHandler(_policyRepo);
                var result = await handler.Execute(query);

                var policy = result.Unwrap();

                _logger.Log("Policy found: {PolicyId}", id);
                return StatusCode(200, policy);
            }
            catch (Exception ex)
            {
                _logger.Exception("Failed to get policy by id", ex.Message);
                return StatusCode(500, "Policy not found");
            }
        }

        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdatePolicy([FromBody] UpdatePolicyCommand data, string id)
        {
            try
            {
                var command = new UpdatePolicyCommand(data.IsActive, data.PolicyCoverageKm, data.PolicyIncidentCoverageAmount, data.PriceExtraKm);

                var validate = _validatorUpdate.Validate(command);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.Error($"Validation failed for UpdatePolicyCommand: {string.Join(", ", errors)}");
                    return StatusCode(400, errors);
                }

                var handler = new UpdatePolicyCommandHandler(_policyRepo);
                var result = await handler.Execute((id, data));
                if (result.IsSuccessful)
                {
                    var policy = result.Unwrap();
                    _logger.Log("Policy updated: {PolicyId}", id);
                    return Ok(policy);
                }
                else
                {
                    _logger.Error("Failed to update policy: {ErrorMessage}", result.ErrorMessage);
                    return StatusCode(409, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.Exception("An error occurred while updating the policy.", ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
