using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Queries.GetAllOrdersByDriverAssigned.Types;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.Types;

namespace OrdersMS.src.Orders.Application.Queries.GetAllOrdersByDriverAssigned
{
    public class GetAllOrdersByDriverAssignedQueryHandler(IOrderRepository orderRepository) : IService<(GetAllOrdersByDriverAssignedQuery data, string driverId), GetOrderResponse[]>
    {
        private readonly IOrderRepository _orderRepository = orderRepository;
        public async Task<Result<GetOrderResponse[]>> Execute((GetAllOrdersByDriverAssignedQuery data, string driverId) request)
        {
            var order = await _orderRepository.GetAllOrdersByDriverAssigned(request.data, request.driverId);
            if (order == null)
            {
                return Result<GetOrderResponse[]>.Failure(new OrderNotFoundException());
            }

            var response = order.Select(order => new GetOrderResponse(
                order.GetId(),
                order.GetContractId(),
                order.GetOperatorAssigned(),
                order.GetDriverAssigned(),
                new CoordinatesDto(order.GetIncidentAddressLatitude(), order.GetIncidentAddressLongitude()),
                new CoordinatesDto(order.GetDestinationAddressLatitude(), order.GetDestinationAddressLongitude()),
                order.GetIncidentType(),
                order.GetIncidentDate(),
                order.GetExtrasServicesApplied().Select(extraCost => new ExtraServiceDto(
                    extraCost.GetId(),
                    extraCost.GetName(),
                    extraCost.GetPrice()
                )).ToList(),
                order.GetTotalCost(),
                order.GetOrderStatus()
            )).ToArray();

            return Result<GetOrderResponse[]>.Success(response);
        }
    }
}
