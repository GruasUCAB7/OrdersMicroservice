using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Commands.UpdateDriverAssigned.Types;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.Types;
using OrdersMS.src.Orders.Domain.ValueObjects;
using RestSharp;
using System.Text.Json;


namespace OrdersMS.src.Orders.Application.Commands.UpdateDriverAssigned
{
    public class UpdateDriverAssignedCommandHandler(IOrderRepository orderRepository, IRestClient restClient) : IService<(string orderId, UpdateDriverAssignedCommand data), GetOrderResponse>
    {
        private readonly IOrderRepository _orderRepository = orderRepository;
        private readonly IRestClient _restClient = restClient;

        public async Task<Result<GetOrderResponse>> Execute((string orderId, UpdateDriverAssignedCommand data) request)
        {
            var orderOptional = await _orderRepository.GetById(request.orderId);
            if (!orderOptional.HasValue)
            {
                return Result<GetOrderResponse>.Failure(new OrderNotFoundException());
            }

            var driverExistsRequest = new RestRequest($"provider/driver/{request.data.DriverAssigned}", Method.Get);
            var responseDriver1 = await _restClient.ExecuteAsync(driverExistsRequest);
            if (!responseDriver1.IsSuccessful)
            {
                throw new Exception($"Failed to get driver information. Content: {responseDriver1.Content}");
            }

            var driverIsAvailableRequest = new RestRequest("provider/provider/availables", Method.Get);
            var responseDriver2 = await _restClient.ExecuteAsync(driverIsAvailableRequest);
            if (!responseDriver2.IsSuccessful)
            {
                throw new Exception($"Failed to get driver availability. Content: {responseDriver2.Content}");
            }

            var driver1 = JsonSerializer.Deserialize<DriverResponse>(responseDriver1.Content ?? string.Empty);
            var availableDrivers = JsonSerializer.Deserialize<List<AvailableDriverResponse>>(responseDriver2.Content ?? string.Empty);


            var driver2 = availableDrivers?.FirstOrDefault(d => d.id == request.data.DriverAssigned);
            if (driver2 == null)
            {
                throw new Exception("Driver not available.");
            }

            if (driver1?.id != driver2.id)
            {
                throw new Exception("Driver IDs do not match.");
            }

            var order = orderOptional.Unwrap();

            if (!string.IsNullOrEmpty(request.data.DriverAssigned))
            {
                order.SetDriverAssigned(new DriverId(request.data.DriverAssigned));
            }

            var updateResult = await _orderRepository.UpdateDriverAssigned(order);
            if (updateResult.IsFailure)
            {
                return Result<GetOrderResponse>.Failure(new OrderUpdateFailedException("The driver assigned of the order could not be updated correctly"));
            }

            var extraServices = order.GetExtrasServicesApplied().Select(extraCost => new ExtraServiceDto(
                extraCost.GetId(),
                extraCost.GetName(),
                extraCost.GetPrice()
            )).ToList();

            var response = new GetOrderResponse(
                order.GetId(),
                order.GetContractId(),
                order.GetDriverAssigned(),
                new CoordinatesDto(order.GetIncidentAddressLatitude(), order.GetIncidentAddressLongitude()),
                new CoordinatesDto(order.GetDestinationAddressLatitude(), order.GetDestinationAddressLongitude()),
                order.GetIncidentDate(),
                extraServices,
                order.GetTotalCost(),
                order.GetOrderStatus()
            );

            return Result<GetOrderResponse>.Success(response);
        }
    }
}
