using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Queries.GetAllOrders.Types;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.Types;

namespace OrdersMS.src.Orders.Application.Queries.GetAllOrders
{
    public class GetAllOrdersQueryHandler(IOrderRepository orderRepository) : IService<GetAllOrdersQuery, GetOrderResponse[]>
    {
        private readonly IOrderRepository _orderRepository = orderRepository;
        public async Task<Result<GetOrderResponse[]>> Execute(GetAllOrdersQuery data)
        {
            var order = await _orderRepository.GetAll(data);
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
