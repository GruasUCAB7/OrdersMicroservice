using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using OrdersMS.Core.Application.GoogleApiService;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Application.Logger;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Orders.Application.Commands.AddExtraCost;
using OrdersMS.src.Orders.Application.Commands.AddExtraCost.Types;
using OrdersMS.src.Orders.Application.Commands.ChangeOrderStatusToAssing;
using OrdersMS.src.Orders.Application.Commands.CreateOrder;
using OrdersMS.src.Orders.Application.Commands.CreateOrder.Types;
using OrdersMS.src.Orders.Application.Commands.UpdateDriverAssigned;
using OrdersMS.src.Orders.Application.Commands.UpdateDriverAssigned.Types;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatus;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatus.Types;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatusToCompleted;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatusToCompleted.Types;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatusToPaid;
using OrdersMS.src.Orders.Application.Commands.UpdateOrderStatusToPaid.Types;
using OrdersMS.src.Orders.Application.Commands.UpdateTotalAmountOrder;
using OrdersMS.src.Orders.Application.Commands.UpdateTotalAmountOrder.Types;
using OrdersMS.src.Orders.Application.Commands.ValidateLocationDriverToIncidecident;
using OrdersMS.src.Orders.Application.Commands.ValidateLocationDriverToIncidecident.Types;
using OrdersMS.src.Orders.Application.Commands.ValidatePricesOfExtrasCost;
using OrdersMS.src.Orders.Application.Commands.ValidatePricesOfExtrasCost.Types;
using OrdersMS.src.Orders.Application.Queries.GetAllOrders;
using OrdersMS.src.Orders.Application.Queries.GetAllOrders.Types;
using OrdersMS.src.Orders.Application.Queries.GetOrderById;
using OrdersMS.src.Orders.Application.Queries.GetOrderById.Types;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.Types;
using OrdersMS.src.Orders.Domain.Services;
using RestSharp;

namespace OrdersMS.src.Orders.Infrastructure.Controller
{
    [Route("order/order")]
    [ApiController]
    public class OrderController(
        IOrderRepository orderRepo,
        IContractRepository contractRepo,
        IPolicyRepository policytRepo,
        IdGenerator<string> idGenerator,
        IValidator<CreateOrderCommand> validatorCreate,
        IValidator<ValidatePricesOfExtrasCostCommand> validatorPricesExtrasCost,
        IValidator<UpdateDriverAssignedCommand> validatorDriverAssigned,
        IValidator<UpdateOrderStatusCommand> validatorOrderStatus,
        IValidator<ValidateLocationCommand> validatorLocationDriver,
        IValidator<UpdateTotalAmountOrderCommand> validatorTotalAmount,
        IValidator<UpdateOrderStatusToCompletedCommand> validatorOrderstatusToCompleted,
        IValidator<UpdateOrderStatusToPaidCommand> validatorOrderstatusToPaid,
        CalculateOrderTotalAmount calculateOrderTotalAmount,
        AddExtraCostCommandHandler addExtraCost,
        IGoogleApiService googleApiService,
        IPublishEndpoint publishEndpoint,
        IRestClient restClient,
        IBus bus,
        ILoggerContract logger) : ControllerBase
    {
        private readonly IOrderRepository _orderRepo = orderRepo;
        private readonly IContractRepository _contractRepo = contractRepo;
        private readonly IPolicyRepository _policyRepo = policytRepo;
        private readonly IdGenerator<string> _idGenerator = idGenerator;
        private readonly IValidator<CreateOrderCommand> _validatorCreate = validatorCreate;
        private readonly IValidator<ValidatePricesOfExtrasCostCommand> _validatorPricesExtrasCost = validatorPricesExtrasCost;
        private readonly IValidator<UpdateDriverAssignedCommand> _validatorDriverAssigned = validatorDriverAssigned;
        private readonly IValidator<UpdateOrderStatusCommand> _validatorOrderStatus = validatorOrderStatus; 
        private readonly IValidator<ValidateLocationCommand> _validatorLocationDriver = validatorLocationDriver; 
        private readonly IValidator<UpdateTotalAmountOrderCommand> _validatorTotalAmount = validatorTotalAmount;
        private readonly IValidator<UpdateOrderStatusToCompletedCommand> _validatorOrderstatusToCompleted = validatorOrderstatusToCompleted;
        private readonly IValidator<UpdateOrderStatusToPaidCommand> _validatorOrderstatusToPaid = validatorOrderstatusToPaid;
        private readonly CalculateOrderTotalAmount _calculateOrderTotalAmount = calculateOrderTotalAmount;
        private readonly AddExtraCostCommandHandler _addExtraCost = addExtraCost;
        private readonly IGoogleApiService _googleApiService = googleApiService;
        private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
        private readonly IRestClient _restClient = restClient;
        private readonly IBus _bus = bus;
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

                var handler = new CreateOrderCommandHandler(_orderRepo, _contractRepo, _idGenerator, _googleApiService, _publishEndpoint);
                var result = await handler.Execute(command);

                if (result.IsSuccessful)
                {
                    _logger.Log("Order created successfully: {OrderId}", result.Unwrap().Id);
                    await _bus.Publish(command);
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

        [HttpPatch("{orderId}/validatePricesExtraCost")]
        public async Task<IActionResult> AddExtraCost([FromBody] ValidatePricesOfExtrasCostCommand data, string orderId)
        {
            try
            {
                var command = new ValidatePricesOfExtrasCostCommand(data.OperatorRespose, data.ExtrasCostApplied);

                var validate = _validatorPricesExtrasCost.Validate(command);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.Error($"Validation failed for ValidatePricesOfExtrasCostCommand: {string.Join(", ", errors)}");
                    return StatusCode(400, errors);
                }

                var handler = new ValidatePricesOfExtrasCostCommandHandler(_orderRepo, _addExtraCost);
                var result = await handler.Execute((orderId, command));
                if (result.IsSuccessful)
                {
                    var order = result.Unwrap();
                    _logger.Log("Order updated: {OrderId}", orderId);
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
                _logger.Exception("An error occurred while updating the extra cost for Order ID: {OrderId}.", orderId, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{orderId}/updateDriverAssigned")]
        public async Task<IActionResult> UpdateDriverAssigned([FromBody] UpdateDriverAssignedCommand data, string orderId)
        {
            try
            {
                var command = new UpdateDriverAssignedCommand(data.DriverAssigned);

                var validate = _validatorDriverAssigned.Validate(command);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.Error($"Validation failed for UpdateDriverAssignedCommand: {string.Join(", ", errors)}");
                    return StatusCode(400, errors);
                }
                var handler = new UpdateDriverAssignedCommandHandler(_orderRepo, _restClient, _publishEndpoint);
                var result = await handler.Execute((orderId, command));
                if (result.IsSuccessful)
                {
                    var order = result.Unwrap();
                    _logger.Log("Driver assigned of the order updated: {OrderId}", orderId);
                    return Ok(order);
                }
                else
                {
                    _logger.Error("Failed to update driver assigned: {ErrorMessage}", result.ErrorMessage);
                    return StatusCode(409, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.Exception("An error occurred while updating the driver assigned for Order ID: {OrderId}.", orderId, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{orderId}/updateOrderStatus")]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusCommand data, string orderId)
        {
            try
            {
                var command = new UpdateOrderStatusCommand(data.OrderAcceptedDriverResponse, data.OrderInProcessDriverResponse, data.OrderCanceledDriverResponse);

                var validate = _validatorOrderStatus.Validate(command);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.Error($"Validation failed for UpdateOrderStatusCommand: {string.Join(", ", errors)}");
                    return StatusCode(400, errors);
                }
                var handler = new UpdateOrderStatusCommandHandler(_orderRepo, _publishEndpoint);
                var result = await handler.Execute((orderId, command));
                if (result.IsSuccessful)
                {
                    var order = result.Unwrap();
                    _logger.Log("Order status updated: {OrderId}", orderId);
                    return Ok(order);
                }
                else
                {
                    _logger.Error("Failed to update order status: {ErrorMessage}", result.ErrorMessage);
                    return StatusCode(409, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.Exception("An error occurred while updating the status for Order ID: {OrderId}.", orderId, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{orderId}/updateOrderStatusToCompleted")]
        public async Task<IActionResult> UpdateOrderStatusToCompleted([FromBody] UpdateOrderStatusToCompletedCommand data, string orderId)
        {
            try
            {
                var command = new UpdateOrderStatusToCompletedCommand(data.DriverAssigned, data.OrderCompletedDriverResponse);

                var validate = _validatorOrderstatusToCompleted.Validate(command);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.Error($"Validation failed for UpdateOrderStatusToCompletedCommand: {string.Join(", ", errors)}");
                    return StatusCode(400, errors);
                }
                var handler = new UpdateOrderStatusToCompletedCommandHandler(_orderRepo, _publishEndpoint);
                var result = await handler.Execute((orderId, command));
                if (result.IsSuccessful)
                {
                    var order = result.Unwrap();
                    _logger.Log("Order status updated: {OrderId}", orderId);
                    return Ok(order);
                }
                else
                {
                    _logger.Error("Failed to update order status: {ErrorMessage}", result.ErrorMessage);
                    return StatusCode(409, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.Exception("An error occurred while updating the status for Order ID: {OrderId}.", orderId, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{orderId}/updateOrderStatusToPaid")]
        public async Task<IActionResult> UpdateOrderStatusToPaid([FromBody] UpdateOrderStatusToPaidCommand data, string orderId)
        {
            try
            {
                var command = new UpdateOrderStatusToPaidCommand(data.OperatorId, data.OrderPaidResponse);

                var validate = _validatorOrderstatusToPaid.Validate(command);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.Error($"Validation failed for UpdateOrderStatusToPaidCommand: {string.Join(", ", errors)}");
                    return StatusCode(400, errors);
                }
                var handler = new UpdateOrderStatusToPaidCommandHandler(_orderRepo, _publishEndpoint);
                var result = await handler.Execute((orderId, command));
                if (result.IsSuccessful)
                {
                    var order = result.Unwrap();
                    _logger.Log("Order status updated: {OrderId}", orderId);
                    return Ok(order);
                }
                else
                {
                    _logger.Error("Failed to update order status: {ErrorMessage}", result.ErrorMessage);
                    return StatusCode(409, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.Exception("An error occurred while updating the status for Order ID: {OrderId}.", orderId, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{orderId}/updateTotalAmountOrder")]
        public async Task<IActionResult> UpdateTotalAmountOrder([FromBody] UpdateTotalAmountOrderCommand data, string orderId)
        {
            try
            {
                var command = new UpdateTotalAmountOrderCommand(data.TotalKmTraveled);

                var validate = _validatorTotalAmount.Validate(command);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.Error($"Validation failed for UpdateTotalAmountOrderCommand: {string.Join(", ", errors)}");
                    return StatusCode(400, errors);
                }
                var handler = new UpdateTotalAmountOrderCommandHandler(_orderRepo, _contractRepo, _policyRepo, _publishEndpoint, _calculateOrderTotalAmount);
                var result = await handler.Execute((orderId, command));
                if (result.IsSuccessful)
                {
                    var order = result.Unwrap();
                    _logger.Log("Order total amount updated: {OrderId}", orderId);
                    return Ok(order);
                }
                else
                {
                    _logger.Error("Failed to update order total amount: {ErrorMessage}", result.ErrorMessage);
                    return StatusCode(409, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.Exception("An error occurred while updating the total amount for Order ID: {OrderId}.", orderId, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("{orderId}/validateDriverLocation")]
        public async Task<IActionResult> ValidateDriverLocation([FromBody] ValidateLocationCommand data, string orderId)
        {
            try
            {
                var command = new ValidateLocationCommand(data.DriverLocationResponse);

                var validate = _validatorLocationDriver.Validate(command);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.Error($"Validation failed for ValidateLocationCommand: {string.Join(", ", errors)}");
                    return StatusCode(400, errors);
                }
                var handler = new ValidateLocationDriverToIncidecidentCommandHandler (_orderRepo, _publishEndpoint);
                var result = await handler.Execute((orderId, command));
                if (result.IsSuccessful)
                {
                    var order = result.Unwrap();
                    _logger.Log("Order status updated: {OrderId}", orderId);
                    return Ok(order);
                }
                else
                {
                    _logger.Error("Failed to update order status: {ErrorMessage}", result.ErrorMessage);
                    return StatusCode(409, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.Exception("An error occurred while updating the status for Order ID: {OrderId}.", orderId, ex.Message);
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut("changeOrderStatusToAssign")]
        public async Task<IActionResult> ValidateUpdateTimeDriver()
        {
            try
            {
                var handler = new ChangeOrderStatusToAssignCommandHandler(_orderRepo);
                var result = await handler.Execute();
                if (result)
                {
                    _logger.Log("Orders updated");
                    return Ok();
                }
                else
                {
                    _logger.Error("Failed to update orders");
                    return StatusCode(409, "Failed to update orders");
                }

            }
            catch (Exception ex)
            {
                _logger.Exception("An error occurred while updating the orders.", ex.Message);
                return StatusCode(500, ex.Message);
            }
        }
    }
}
