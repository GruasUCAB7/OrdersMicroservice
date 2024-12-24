using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Application.Logger;
using OrdersMS.src.Contracts.Application.Commands.CreateInsuredVehicle;
using OrdersMS.src.Contracts.Application.Commands.CreateInsuredVehicle.Types;
using OrdersMS.src.Contracts.Application.Commands.UpdateInsuredVehicle;
using OrdersMS.src.Contracts.Application.Commands.UpdateInsuredVehicle.Types;
using OrdersMS.src.Contracts.Application.Queries.GetAllVehicles;
using OrdersMS.src.Contracts.Application.Queries.GetAllVehicles.Types;
using OrdersMS.src.Contracts.Application.Queries.GetVehicleById;
using OrdersMS.src.Contracts.Application.Queries.GetVehicleById.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Application.Types;

namespace OrdersMS.src.Contracts.Infrastructure.Controllers
{
    [Route("orders/vehicle")]
    [ApiController]
    public class InsuredVehicleController(
        IInsuredVehicleRepository vehicleRepo,
        IdGenerator<string> idGenerator,
        IValidator<CreateVehicleCommand> validatorCreate,
        IValidator<UpdateVehicleCommand> validatorUpdate,
        ILoggerContract logger) : ControllerBase
    {
        private readonly IInsuredVehicleRepository _vehicleRepo = vehicleRepo;
        private readonly IdGenerator<string> _idGenerator = idGenerator;
        private readonly IValidator<CreateVehicleCommand> _validatorCreate = validatorCreate;
        private readonly IValidator<UpdateVehicleCommand> _validatorUpdate = validatorUpdate;
        private readonly ILoggerContract _logger = logger;

        [HttpPost]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleCommand data)
        {
            try
            {
                var command = new CreateVehicleCommand(data.Brand, data.Model, data.Plate, data.VehicleSize, data.Year, data.ClientDNI, data.ClientName, data.ClientEmail);

                var validate = _validatorCreate.Validate(command);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.Error($"Validation failed for CreateVehicleCommand: {string.Join(", ", errors)}");
                    return StatusCode(400, errors);
                }

                var handler = new CreateVehicleCommandHandler(_vehicleRepo, _idGenerator);
                var result = await handler.Execute(command);

                if (result.IsSuccessful)
                {
                    _logger.Log("Vehicle created successfully: {VehicleId}", result.Unwrap().Id);
                    return StatusCode(201, new { id = result.Unwrap().Id });
                }
                else
                {
                    _logger.Error("Failed to create vehicle: {ErrorMessage}", result.ErrorMessage);
                    return StatusCode(409, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.Exception("An error occurred while creating the vehicle.", ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllVehicles([FromQuery] GetAllVehiclesQuery data)
        {
            try
            {
                var query = new GetAllVehiclesQuery(data.PerPage, data.Page, data.IsActive);
                var handler = new GetAllVehiclesQueryHandler(_vehicleRepo);
                var result = await handler.Execute(query);

                _logger.Log("List of vehicles: {VehicleIds}", string.Join(", ", result.Unwrap().Select(c => c.Id)));
                return StatusCode(200, result.Unwrap());
            }
            catch (Exception ex)
            {
                _logger.Exception("Failed to get list of vehicles", ex.Message);
                return StatusCode(200, Array.Empty<GetVehicleResponse>());
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleById(string id)
        {
            try
            {
                var query = new GetVehicleByIdQuery(id);
                var handler = new GetVehicleByIdQueryHandler(_vehicleRepo);
                var result = await handler.Execute(query);

                var vehicle = result.Unwrap();

                _logger.Log("Vehicle found: {VehicleId}", id);
                return StatusCode(200, vehicle);
            }
            catch (Exception ex)
            {
                _logger.Exception("Failed to get vehicle by id", ex.Message);
                return StatusCode(500, "Vehicle not found");
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateVehicle([FromBody] UpdateVehicleCommand data, string id)
        {
            try
            {
                var command = new UpdateVehicleCommand(data.IsActive);

                var validate = _validatorUpdate.Validate(command);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.Error($"Validation failed for UpdateVehicleCommand: {string.Join(", ", errors)}");
                    return StatusCode(400, errors);
                }

                var handler = new UpdateVehicleCommandHandler(_vehicleRepo);
                var result = await handler.Execute((id, data));
                if (result.IsSuccessful)
                {
                    var vehicle = result.Unwrap();
                    _logger.Log("Vehicle updated: {VehicleId}", id);
                    return Ok(vehicle);
                }
                else
                {
                    _logger.Error("Failed to update vehicle: {ErrorMessage}", result.ErrorMessage);
                    return StatusCode(409, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.Exception("An error occurred while updating the vehicle.", ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
