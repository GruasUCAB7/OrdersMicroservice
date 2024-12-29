using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Application.Logger;
using OrdersMS.src.Contracts.Application.Commands.CreateContract;
using OrdersMS.src.Contracts.Application.Commands.CreateContract.Types;
using OrdersMS.src.Contracts.Application.Commands.UpdateContract;
using OrdersMS.src.Contracts.Application.Commands.UpdateContract.Types;
using OrdersMS.src.Contracts.Application.Queries.GetAllContracts;
using OrdersMS.src.Contracts.Application.Queries.GetAllContracts.Types;
using OrdersMS.src.Contracts.Application.Queries.GetContractById;
using OrdersMS.src.Contracts.Application.Queries.GetContractById.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Application.Types;

namespace OrdersMS.src.Contracts.Infrastructure.Controllers
{
    [Route("order/contract")]
    [ApiController]
    public class ContractController(
        IContractRepository contractRepo,
        IPolicyRepository policyRepo,
        IInsuredVehicleRepository vehicleRepo,
        IdGenerator<string> idGenerator,
        IValidator<CreateContractCommand> validatorCreate,
        IValidator<UpdateContractCommand> validatorUpdate,
        ILoggerContract logger) : ControllerBase
    {
        private readonly IContractRepository _contractRepo = contractRepo;
        private readonly IPolicyRepository _policyRepo = policyRepo;
        private readonly IInsuredVehicleRepository _vehicleRepo = vehicleRepo;
        private readonly IdGenerator<string> _idGenerator = idGenerator;
        private readonly IValidator<CreateContractCommand> _validatorCreate = validatorCreate;
        private readonly IValidator<UpdateContractCommand> _validatorUpdate = validatorUpdate;
        private readonly ILoggerContract _logger = logger;

        [HttpPost]
        public async Task<IActionResult> CreateContract([FromBody] CreateContractCommand data)
        {
            try
            {
                var command = new CreateContractCommand(data.AssociatedPolicy, data.InsuredVehicle);

                var validate = _validatorCreate.Validate(command);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.Error($"Validation failed for CreateContractCommand: {string.Join(", ", errors)}");
                    return StatusCode(400, errors);
                }

                var handler = new CreateContractCommandHandler(_contractRepo, _vehicleRepo, _policyRepo, _idGenerator);
                var result = await handler.Execute(command);

                if (result.IsSuccessful)
                {
                    _logger.Log("Contract created successfully: {ContractId}", result.Unwrap().Id);
                    return StatusCode(201, new { id = result.Unwrap().Id });
                }
                else
                {
                    _logger.Error("Failed to create contract: {ErrorMessage}", result.ErrorMessage);
                    return StatusCode(409, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.Exception("An error occurred while creating the contract.", ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllContracts([FromQuery] GetAllContractsQuery data)
        {
            try
            {
                var query = new GetAllContractsQuery(data.PerPage, data.Page, data.Status);
                var handler = new GetAllContractsQueryHandler(_contractRepo);
                var result = await handler.Execute(query);

                _logger.Log("List of contracts: {ContractIds}", string.Join(", ", result.Unwrap().Select(c => c.Id)));
                return StatusCode(200, result.Unwrap());
            }
            catch (Exception ex)
            {
                _logger.Exception("Failed to get list of contracts", ex.Message);
                return StatusCode(200, Array.Empty<GetContractResponse>());
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContractById(string id)
        {
            try
            {
                var query = new GetContractByIdQuery(id);
                var handler = new GetContractByIdQueryHandler(_contractRepo);
                var result = await handler.Execute(query);

                var contract = result.Unwrap();

                _logger.Log("Contract found: {ContractId}", id);
                return StatusCode(200, contract);
            }
            catch (Exception ex)
            {
                _logger.Exception("Failed to get contract by id", ex.Message);
                return StatusCode(500, "Contract not found");
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateContract([FromBody] UpdateContractCommand data, string id)
        {
            try
            {
                var command = new UpdateContractCommand(data.Status);

                var validate = _validatorUpdate.Validate(command);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.Error($"Validation failed for UpdateContractCommand: {string.Join(", ", errors)}");
                    return StatusCode(400, errors);
                }

                var handler = new UpdateContractCommandHandler(_contractRepo);
                var result = await handler.Execute((id, data));
                if (result.IsSuccessful)
                {
                    var contract = result.Unwrap();
                    _logger.Log("Contract updated: {ContractId}", id);
                    return Ok(contract);
                }
                else
                {
                    _logger.Error("Failed to update contract: {ErrorMessage}", result.ErrorMessage);
                    return StatusCode(409, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.Exception("An error occurred while updating the contract.", ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
