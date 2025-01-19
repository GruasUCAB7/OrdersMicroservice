using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Queries.GetExtraCostsByOrderId.Types;
using OrdersMS.src.Orders.Application.Repositories;

namespace OrdersMS.src.Orders.Application.Queries.GetExtraCostsByOrderId
{
    public class GetExtraCostsByOrderIdQueryHandler(IExtraCostRepository extraCostRepository)
    {
        private readonly IExtraCostRepository _extraCostRepository = extraCostRepository;
        public async Task<Result<GetExtraCostResponse>> Execute(string orderId)
        {
            var extraCosts = await _extraCostRepository.GetExtraCostByOrderId(orderId);
            if (extraCosts == null)
            {
                return Result<GetExtraCostResponse>.Failure(new ExtraCostNotFoundException());
            }

            var extraCostDtos = extraCosts.Select(extraCost => new ExtraCostDto(
                extraCost.GetId(),
                extraCost.GetName(),
                extraCost.GetPrice()
            )).ToList();

            var response = new GetExtraCostResponse(extraCostDtos);

            return Result<GetExtraCostResponse>.Success(response);
        }
    }
}
