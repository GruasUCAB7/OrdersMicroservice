using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OrdersMS.Core.Application.GoogleApiService;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Application.Logger;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Orders.Application.Commands.AddExtraCost;
using OrdersMS.src.Orders.Application.Commands.AddExtraCost.Types;
using OrdersMS.src.Orders.Application.Commands.CreateOrder;
using OrdersMS.src.Orders.Application.Commands.CreateOrder.Types;
using OrdersMS.src.Orders.Application.Queries.GetAllOrders;
using OrdersMS.src.Orders.Application.Queries.GetAllOrders.Types;
using OrdersMS.src.Orders.Application.Queries.GetOrderById;
using OrdersMS.src.Orders.Application.Queries.GetOrderById.Types;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.Types;

namespace OrdersMS.src.Orders.Infrastructure.Controller
{
    [Route("order/order")]
    [ApiController]
    public class OrderController(
        IOrderRepository orderRepo,
        IContractRepository contractRepo,
        IdGenerator<string> idGenerator,
        IValidator<CreateOrderCommand> validatorCreate,
        IValidator<AddExtraCostCommand> validatorAddExtraCost,
        IGoogleApiService googleApiService,
        ILoggerContract logger) : ControllerBase
    {
        private readonly IOrderRepository _orderRepo = orderRepo;
        private readonly IContractRepository _contractRepo = contractRepo;
        private readonly IdGenerator<string> _idGenerator = idGenerator;
        private readonly IValidator<CreateOrderCommand> _validatorCreate = validatorCreate;
        private readonly IValidator<AddExtraCostCommand> _validatorAddExtraCost = validatorAddExtraCost;
        private readonly IGoogleApiService _googleApiService = googleApiService;
        private readonly ILoggerContract _logger = logger;

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand data)
        {
            try
            {
                var command = new CreateOrderCommand(data.ContractId, data.DriverAssigned, data.IncidentAddress, data.DestinationAddress, data.ExtraServicesApplied);

                var validate = _validatorCreate.Validate(command);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.Error($"Validation failed for CreateOrderCommand: {string.Join(", ", errors)}");
                    return StatusCode(400, errors);
                }

                var handler = new CreateOrderCommandHandler(_orderRepo, _contractRepo, _idGenerator, _googleApiService);
                var result = await handler.Execute(command);

                if (result.IsSuccessful)
                {
                    _logger.Log("Order created successfully: {OrderId}", result.Unwrap().Id);
                    return StatusCode(201, new { id = result.Unwrap().Id });
                }
                else
                {
                    _logger.Error("Failed to create order: {ErrorMessage}", result.ErrorMessage);
                    return StatusCode(409, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.Exception("An error occurred while creating the order.", ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders([FromQuery] GetAllOrdersQuery data)
        {
            try
            {
                var query = new GetAllOrdersQuery(data.PerPage, data.Page, data.Status);
                var handler = new GetAllOrdersQueryHandler(_orderRepo);
                var result = await handler.Execute(query);

                _logger.Log("List of orders: {OrderIds}", string.Join(", ", result.Unwrap().Select(c => c.Id)));
                return StatusCode(200, result.Unwrap());
            }
            catch (Exception ex)
            {
                _logger.Exception("Failed to get list of orders", ex.Message);
                return StatusCode(200, Array.Empty<GetOrderResponse>());
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(string id)
        {
            try
            {
                var query = new GetOrderByIdQuery(id);
                var handler = new GetOrderByIdQueryHandler(_orderRepo);
                var result = await handler.Execute(query);

                var order = result.Unwrap();

                _logger.Log("Order found: {OrderId}", id);
                return StatusCode(200, order);
            }
            catch (Exception ex)
            {
                _logger.Exception("Failed to get order by id", ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPatch]
        public async Task<IActionResult> UpdateExtraCost([FromBody] AddExtraCostCommand data)
        {
            try
            {
                var command = new AddExtraCostCommand(data.OrderId, data.ExtraCosts);

                var validate = _validatorAddExtraCost.Validate(command);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.Error($"Validation failed for AddExtraCostCommand: {string.Join(", ", errors)}");
                    return StatusCode(400, errors);
                }

                var handler = new AddExtraCostCommandHandler(_orderRepo, _idGenerator);
                var result = await handler.Execute(command);
                if (result.IsSuccessful)
                {
                    var order = result.Unwrap();
                    _logger.Log("Order updated: {OrderId}", data.OrderId);
                    return Ok(order);
                }
                else
                {
                    _logger.Error("Failed to update extra cost: {ErrorMessage}", result.ErrorMessage);
                    return StatusCode(409, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.Exception("An error occurred while updating the extra cost for Order ID: {OrderId}.", data.OrderId, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
