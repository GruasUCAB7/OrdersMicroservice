using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Queries.GetExtraCostById.Types;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.Types;

namespace OrdersMS.src.Orders.Application.Queries.GetExtraCostById
{
    public class GetExtraCostByIdQueryHandler(IExtraCostRepository extraCostRepository) : IService<GetExtraCostByIdQuery, GetExtraCostResponse>
    {
        private readonly IExtraCostRepository _extraCostRepository = extraCostRepository;
        public async Task<Result<GetExtraCostResponse>> Execute(GetExtraCostByIdQuery data)
        {
            var extraCostOptional = await _extraCostRepository.GetById(data.Id);
            if (!extraCostOptional.HasValue)
            {
                return Result<GetExtraCostResponse>.Failure(new ExtraCostNotFoundException());
            }

            var extraCost = extraCostOptional.Unwrap();
            var response = new GetExtraCostResponse(
                extraCost.GetId(),
                extraCost.GetName(),
                extraCost.GetIsActive()
            );

            return Result<GetExtraCostResponse>.Success(response);
        }
    }
}
