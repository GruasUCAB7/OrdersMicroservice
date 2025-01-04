using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Queries.GetOrderById.Types;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.Types;

namespace OrdersMS.src.Orders.Application.Queries.GetOrderById
{
    public class GetOrderByIdQueryHandler(IOrderRepository orderRepository) : IService<GetOrderByIdQuery, GetOrderResponse>
    {
        private readonly IOrderRepository _orderRepository = orderRepository;
        public async Task<Result<GetOrderResponse>> Execute(GetOrderByIdQuery data)
        {
            var orderOptional = await _orderRepository.GetById(data.Id);
            if (!orderOptional.HasValue)
            {
                return Result<GetOrderResponse>.Failure(new OrderNotFoundException());
            }

            var order = orderOptional.Unwrap();

            var extraServices = order.GetExtrasServicesApplied().Select(extraCost => new ExtraServiceDto(
                extraCost.GetId(),
                extraCost.GetName(),
                extraCost.GetPrice()
            )).ToList();

            var response = new GetOrderResponse(
               order.GetId(),
               order.GetContractId(),
               order.GetOperatorAssigned(),
               order.GetDriverAssigned(),
               new CoordinatesDto(order.GetIncidentAddressLatitude(), order.GetIncidentAddressLongitude()),
               new CoordinatesDto(order.GetDestinationAddressLatitude(), order.GetDestinationAddressLongitude()),
               order.GetIncidentType(),
               order.GetIncidentDate(),
               extraServices,
               order.GetTotalCost(),
               order.GetOrderStatus()
           );

            return Result<GetOrderResponse>.Success(response);
        }
    }
}
