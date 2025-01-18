using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrdersMS.Core.Application.GoogleApiService;
using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Application.Logger;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Orders.Application.Commands.AddExtraCost;
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
using OrdersMS.src.Orders.Application.Commands.ValidateLocationDriverToIncidecident.Types;
using OrdersMS.src.Orders.Application.Commands.ValidateLocationDriverToIncident;
using OrdersMS.src.Orders.Application.Commands.ValidatePricesOfExtrasCost;
using OrdersMS.src.Orders.Application.Commands.ValidatePricesOfExtrasCost.Types;
using OrdersMS.src.Orders.Application.Queries.GetAllOrders;
using OrdersMS.src.Orders.Application.Queries.GetAllOrders.Types;
using OrdersMS.src.Orders.Application.Queries.GetAllOrdersByDriverAssigned;
using OrdersMS.src.Orders.Application.Queries.GetAllOrdersByDriverAssigned.Types;
using OrdersMS.src.Orders.Application.Queries.GetOrderById;
using OrdersMS.src.Orders.Application.Queries.GetOrderById.Types;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.Types;
using OrdersMS.src.Orders.Domain.Services;
using OrdersMS.src.Orders.Infrastructure.Types;
using RestSharp;
using System.Text.Json;


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
        [Authorize(Roles = "Admin, Operator")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand data, [FromHeader(Name = "Authorization")] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return StatusCode(400, "Invalid or missing Authorization header");
                }

                var operatorExist = new RestRequest($"https://localhost:4051/user/{data.OperatorId}", Method.Get);
                operatorExist.AddHeader("Authorization", token);
                var response = await _restClient.ExecuteAsync(operatorExist);
                if (!response.IsSuccessful)
                {
                    throw new Exception($"Failed to get user id. Content: {response.Content}");
                }

                var command = new CreateOrderCommand(data.ContractId, data.OperatorId, data.DriverAssigned, data.IncidentAddress, data.DestinationAddress, data.IncidentType, data.ExtraServicesApplied);

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
        [Authorize(Roles = "Admin, Operator, Driver")]
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


        [HttpGet("ordersByDriverId/{driverId}")]
        [Authorize(Roles = "Admin, Operator, Driver")]
        public async Task<IActionResult> GetAllOrdersByDriverAssigned([FromQuery] GetAllOrdersByDriverAssignedQuery data, string driverId)
        {
            try
            {
                var query = new GetAllOrdersByDriverAssignedQuery(data.PerPage, data.Page);
                var handler = new GetAllOrdersByDriverAssignedQueryHandler(_orderRepo);
                var result = await handler.Execute((query, driverId));

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
        [Authorize(Roles = "Admin, Operator, Driver")]
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
        [Authorize(Roles = "Admin, Operator")]
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
        [Authorize(Roles = "Admin, Operator")]
        public async Task<IActionResult> UpdateDriverAssigned([FromBody] UpdateDriverAssignedCommand data, string orderId, [FromHeader(Name = "Authorization")] string token)
        {
            try
            {
                var driverExistsRequest = new RestRequest($"https://localhost:4052/provider/driver/{data.DriverAssigned}", Method.Get);
                driverExistsRequest.AddHeader("Authorization", token);
                var responseDriver1 = await _restClient.ExecuteAsync(driverExistsRequest);
                if (!responseDriver1.IsSuccessful)
                {
                    throw new Exception($"Failed to get driver information. Content: {responseDriver1.Content}");
                }

                var driverIsAvailableRequest = new RestRequest("https://localhost:4052/provider/provider/availables", Method.Get);
                driverIsAvailableRequest.AddHeader("Authorization", token);
                var responseDriver2 = await _restClient.ExecuteAsync(driverIsAvailableRequest);
                if (!responseDriver2.IsSuccessful)
                {
                    throw new Exception($"Failed to get driver availability. Content: {responseDriver2.Content}");
                }

                var driver1 = JsonSerializer.Deserialize<DriverResponse>(responseDriver1.Content ?? string.Empty);
                var availableDrivers = JsonSerializer.Deserialize<List<AvailableDriverResponse>>(responseDriver2.Content ?? string.Empty);

                var driver2 = availableDrivers?.FirstOrDefault(d => d.id == data.DriverAssigned);
                if (driver2 == null)
                {
                    throw new Exception("Driver not available.");
                }

                if (driver1?.id != driver2.id)
                {
                    throw new Exception("Driver IDs do not match.");
                }

                var command = new UpdateDriverAssignedCommand(data.DriverAssigned);

                var validate = _validatorDriverAssigned.Validate(command);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => e.ErrorMessage).ToList();
                    _logger.Error($"Validation failed for UpdateDriverAssignedCommand: {string.Join(", ", errors)}");
                    return StatusCode(400, errors);
                }

                var handler = new UpdateDriverAssignedCommandHandler(_orderRepo, _publishEndpoint);
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
        [Authorize(Roles = "Admin, Operator, Driver")]
        public async Task<IActionResult> UpdateOrderStatus([FromBody] UpdateOrderStatusCommand data, string orderId, [FromHeader(Name = "Authorization")] string token)
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
                var order = result.Unwrap();

                if (data.OrderAcceptedDriverResponse == true)
                {
                    var changeIsAvailableToTrueDriver = new RestRequest($"https://localhost:4052/provider/driver/{order.DriverAssigned}", Method.Patch);
                    changeIsAvailableToTrueDriver.AddHeader("Authorization", token);
                    changeIsAvailableToTrueDriver.AddJsonBody(new { isAvailable = false });

                    var responseDriver = await _restClient.ExecuteAsync(changeIsAvailableToTrueDriver);
                    if (!responseDriver.IsSuccessful)
                    {
                        throw new Exception($"Failed to update driver availability. Content: {responseDriver.Content}");
                    }
                }

                if (data.OrderCanceledDriverResponse == true)
                {
                    var changeIsAvailableToTrueDriver = new RestRequest($"https://localhost:4052/provider/driver/{order.DriverAssigned}", Method.Patch);
                    changeIsAvailableToTrueDriver.AddHeader("Authorization", token);
                    changeIsAvailableToTrueDriver.AddJsonBody(new { isAvailable = true });

                    var responseDriver = await _restClient.ExecuteAsync(changeIsAvailableToTrueDriver);
                    if (!responseDriver.IsSuccessful)
                    {
                        throw new Exception($"Failed to update driver availability. Content: {responseDriver.Content}");
                    }
                }

                if (result.IsSuccessful)
                {
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
        [Authorize(Roles = "Admin, Operator, Driver")]
        public async Task<IActionResult> UpdateOrderStatusToCompleted([FromBody] UpdateOrderStatusToCompletedCommand data, string orderId, [FromHeader(Name = "Authorization")] string token)
        {
            try
            {
                var command = new UpdateOrderStatusToCompletedCommand(data.OrderCompletedDriverResponse);

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

                    var changeIsAvailableToTrueDriver = new RestRequest($"https://localhost:4052/provider/driver/{order.DriverAssigned}", Method.Patch);
                    changeIsAvailableToTrueDriver.AddHeader("Authorization", token);
                    changeIsAvailableToTrueDriver.AddJsonBody(new { isAvailable = true });

                    var responseDriver1 = await _restClient.ExecuteAsync(changeIsAvailableToTrueDriver);
                    if (!responseDriver1.IsSuccessful)
                    {
                        throw new Exception($"Failed to update driver availability. Content: {responseDriver1.Content}");
                    }

                    var changeLocationDriver = new RestRequest($"https://localhost:4052/provider/driver/{order.DriverAssigned}", Method.Put);
                    changeLocationDriver.AddHeader("Authorization", token);
                    changeLocationDriver.AddJsonBody(new { latitude = order.DestinationAddress.Latitude, longitude = order.DestinationAddress.Longitude });

                    var responseDriver2 = await _restClient.ExecuteAsync(changeLocationDriver);
                    if (!responseDriver2.IsSuccessful)
                    {
                        throw new Exception($"Failed to update driver location. Content: {responseDriver2.Content}");
                    }

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
        [Authorize(Roles = "Admin, Operator")]
        public async Task<IActionResult> UpdateOrderStatusToPaid([FromBody] UpdateOrderStatusToPaidCommand data, string orderId)
        {
            try
            {
                var command = new UpdateOrderStatusToPaidCommand(data.OrderPaidResponse);

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
        [Authorize(Roles = "Admin, Operator, Driver")]
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
                var handler = new UpdateTotalAmountOrderCommandHandler(_orderRepo, _contractRepo, _policyRepo, _calculateOrderTotalAmount);
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
        [Authorize(Roles = "Admin, Operator, Driver")]
        public async Task<IActionResult> ValidateDriverLocation([FromBody] ValidateLocationCommand data, string orderId, [FromHeader(Name = "Authorization")] string token)
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
                var handler = new ValidateLocationDriverToIncidentCommandHandler(_orderRepo, _publishEndpoint);
                var result = await handler.Execute((orderId, command));
                if (result.IsSuccessful)
                {
                    var order = result.Unwrap();

                    var changeLocationDriver = new RestRequest($"https://localhost:4052/provider/driver/{order.DriverAssigned}", Method.Put);
                    changeLocationDriver.AddHeader("Authorization", token);
                    changeLocationDriver.AddJsonBody(new { latitude = order.IncidentAddress.Latitude, longitude = order.IncidentAddress.Longitude });

                    var responseDriver = await _restClient.ExecuteAsync(changeLocationDriver);
                    if (!responseDriver.IsSuccessful)
                    {
                        throw new Exception($"Failed to update driver location. Content: {responseDriver.Content}");
                    }

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
        public async Task<IActionResult> ValidateUpdateTimeDriver([FromHeader(Name = "Authorization")] string token)
        {
            try
            {
                var handler = new ChangeOrderStatusToAssignCommandHandler(_orderRepo);
                var result = await handler.Execute();

                if (result.IsSuccessful)
                {
                    var modifiedOrders = result.Unwrap();

                    foreach (var order in modifiedOrders)
                    {
                        var driverId = order.GetDriverAssigned();
                        var changeIsAvailableToTrueDriver = new RestRequest($"https://localhost:4052/provider/driver/{driverId}", Method.Patch);
                        changeIsAvailableToTrueDriver.AddHeader("Authorization", token);
                        changeIsAvailableToTrueDriver.AddJsonBody(new { isAvailable = true });

                        var responseDriver2 = await _restClient.ExecuteAsync(changeIsAvailableToTrueDriver);
                        if (!responseDriver2.IsSuccessful)
                        {
                            throw new Exception($"Failed to update driver availability for driver ID: {driverId}. Content: {responseDriver2.Content}");
                        }
                    }

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
