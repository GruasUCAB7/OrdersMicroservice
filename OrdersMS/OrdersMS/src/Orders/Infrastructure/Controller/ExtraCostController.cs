using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Application.Logger;
using OrdersMS.src.Orders.Application.Commands.CreateExtraCost;
using OrdersMS.src.Orders.Application.Commands.CreateExtraCost.Types;
using OrdersMS.src.Orders.Application.Commands.UpdateExtraCost;
using OrdersMS.src.Orders.Application.Commands.UpdateExtraCost.Types;
using OrdersMS.src.Orders.Application.Queries.GetAllExtraCosts;
using OrdersMS.src.Orders.Application.Queries.GetAllExtraCosts.Types;
using OrdersMS.src.Orders.Application.Queries.GetExtraCostById;
using OrdersMS.src.Orders.Application.Queries.GetExtraCostById.Types;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.Types;

namespace OrdersMS.src.Orders.Infrastructure.Controller
{
    [Route("order/extraCost")]
    [ApiController]
    public class ExtraCostController(
        IExtraCostRepository extraCostRepo,
        IdGenerator<string> idGenerator,
        IValidator<CreateExtraCostCommand> validatorCreate,
        IValidator<UpdateExtraCostCommand> validatorUpdate,
        ILoggerContract logger) : ControllerBase
    {
        private readonly IExtraCostRepository _extraCostRepo = extraCostRepo;
        private readonly IdGenerator<string> _idGenerator = idGenerator;
        private readonly IValidator<CreateExtraCostCommand> _validatorCreate = validatorCreate;
        private readonly IValidator<UpdateExtraCostCommand> _validatorUpdate = validatorUpdate;
        private readonly ILoggerContract _logger = logger;

        [HttpPost]
        public async Task<IActionResult> CreateExtraCost([FromBody] CreateExtraCostCommand data)
        {
            try
            {
                var command = new CreateExtraCostCommand(data.Name);

                var validate = _validatorCreate.Validate(command);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.Error($"Validation failed for CreateExtraCostCommand: {string.Join(", ", errors)}");
                    return StatusCode(400, errors);
                }

                var handler = new CreateExtraCostCommandHandler(_extraCostRepo, _idGenerator);
                var result = await handler.Execute(command);

                if (result.IsSuccessful)
                {
                    _logger.Log("Extra cost created successfully: {ExtraCostId}", result.Unwrap().Id);
                    return StatusCode(201, new { id = result.Unwrap().Id });
                }
                else
                {
                    _logger.Error("Failed to create extra cost: {ErrorMessage}", result.ErrorMessage);
                    return StatusCode(409, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.Exception("An error occurred while creating the extra cost.", ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllExtraCosts([FromQuery] GetAllExtraCostsQuery data)
        {
            try
            {
                var query = new GetAllExtraCostsQuery(data.PerPage, data.Page, data.IsActive);
                var handler = new GetAllExtraCostsQueryHandler(_extraCostRepo);
                var result = await handler.Execute(query);

                _logger.Log("List of extra costs: {ExtraCostIds}", string.Join(", ", result.Unwrap().Select(c => c.Id)));
                return StatusCode(200, result.Unwrap());
            }
            catch (Exception ex)
            {
                _logger.Exception("Failed to get list of extra costs", ex.Message);
                return StatusCode(200, Array.Empty<GetExtraCostResponse>());
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetExtraCostById(string id)
        {
            try
            {
                var query = new GetExtraCostByIdQuery(id);
                var handler = new GetExtraCostByIdQueryHandler(_extraCostRepo);
                var result = await handler.Execute(query);

                var extraCost = result.Unwrap();

                _logger.Log("Extra cost found: {ExtraCostId}", id);
                return StatusCode(200, extraCost);
            }
            catch (Exception ex)
            {
                _logger.Exception("Failed to get extra cost by id", ex.Message);
                return StatusCode(500, "Extra cost not found");
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateExtraCost([FromBody] UpdateExtraCostCommand data, string id)
        {
            try
            {
                var command = new UpdateExtraCostCommand(data.IsActive);

                var validate = _validatorUpdate.Validate(command);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.Error($"Validation failed for UpdateExtraCostCommand: {string.Join(", ", errors)}");
                    return StatusCode(400, errors);
                }

                var handler = new UpdateExtraCostCommandHandler(_extraCostRepo);
                var result = await handler.Execute((id, data));
                if (result.IsSuccessful)
                {
                    var extraCost = result.Unwrap();
                    _logger.Log("Extra cost updated: {ExtraCostId}", id);
                    return Ok(extraCost);
                }
                else
                {
                    _logger.Error("Failed to update extra cost: {ErrorMessage}", result.ErrorMessage);
                    return StatusCode(409, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.Exception("An error occurred while updating the extra cost.", ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
