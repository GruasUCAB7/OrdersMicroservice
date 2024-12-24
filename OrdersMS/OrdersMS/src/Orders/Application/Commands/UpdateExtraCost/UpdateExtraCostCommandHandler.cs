using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Orders.Application.Commands.UpdateExtraCost.Types;
using OrdersMS.src.Orders.Application.Exceptions;
using OrdersMS.src.Orders.Application.Repositories;

namespace OrdersMS.src.Orders.Application.Commands.UpdateExtraCost
{
    public class UpdateExtraCostCommandHandler(IExtraCostRepository extraCostRepository) : IService<(string id, UpdateExtraCostCommand data), UpdateExtraCostResponse>
    {
        private readonly IExtraCostRepository _extraCostRepository = extraCostRepository;

        public async Task<Result<UpdateExtraCostResponse>> Execute((string id, UpdateExtraCostCommand data) request)
        {
            var extraCostOptional = await _extraCostRepository.GetById(request.id);
            if (!extraCostOptional.HasValue)
            {
                return Result<UpdateExtraCostResponse>.Failure(new ExtraCostNotFoundException());
            }

            var extraCost = extraCostOptional.Unwrap();

            if (request.data.IsActive.HasValue)
            {
                extraCost.SetIsActive(request.data.IsActive.Value);
            }

            var updateResult = await _extraCostRepository.Update(extraCost);
            if (updateResult.IsFailure)
            {
                return Result<UpdateExtraCostResponse>.Failure(new ExtraCostUpdateFailedException());
            }

            var response = new UpdateExtraCostResponse(
                extraCost.GetId(),
                extraCost.GetName(),
                extraCost.GetIsActive()
            );

            return Result<UpdateExtraCostResponse>.Success(response);
        }
    }
}
