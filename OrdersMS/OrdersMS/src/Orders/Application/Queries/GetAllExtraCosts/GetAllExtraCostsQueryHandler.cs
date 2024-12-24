using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Queries.GetAllExtraCosts.Types;
using OrdersMS.src.Orders.Application.Repositories;
using OrdersMS.src.Orders.Application.Types;

namespace OrdersMS.src.Orders.Application.Queries.GetAllExtraCosts
{
    public class GetAllExtraCostsQueryHandler(IExtraCostRepository extraCostRepository) : IService<GetAllExtraCostsQuery, GetExtraCostResponse[]>
    {
        private readonly IExtraCostRepository _extraCostRepository = extraCostRepository;
        public async Task<Result<GetExtraCostResponse[]>> Execute(GetAllExtraCostsQuery data)
        {
            var extraCost = await _extraCostRepository.GetAll(data);
            if (extraCost == null)
            {
                return Result<GetExtraCostResponse[]>.Failure(new ExtraCostNotFoundException());
            }

            var response = extraCost.Select(extraCost => new GetExtraCostResponse(
                extraCost.GetId(),
                extraCost.GetName(),
                extraCost.GetIsActive()
                )
            ).ToArray();

            return Result<GetExtraCostResponse[]>.Success(response);
        }
    }
}
