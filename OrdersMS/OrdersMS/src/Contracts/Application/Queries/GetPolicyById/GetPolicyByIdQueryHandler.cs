using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Exceptions;
using OrdersMS.src.Contracts.Application.Queries.GetPolicyById.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Application.Types;

namespace OrdersMS.src.Contracts.Application.Queries.GetPolicyById
{
    public class GetPolicyByIdQueryHandler(IPolicyRepository policyRepository) : IService<GetPolicyByIdQuery, GetPolicyResponse>
    {
        private readonly IPolicyRepository _policyRepository = policyRepository;
        public async Task<Result<GetPolicyResponse>> Execute(GetPolicyByIdQuery data)
        {
            var vehicleOptional = await _policyRepository.GetById(data.Id);
            if (!vehicleOptional.HasValue)
            {
                return Result<GetPolicyResponse>.Failure(new PolicyNotFoundException());
            }

            var policy = vehicleOptional.Unwrap();
            var response = new GetPolicyResponse(
                policy.GetId(),
                policy.GetPolicyType(),
                policy.GetPolicyCoverageKm(),
                policy.GetPolicyIncidentCoverageAmount(),
                policy.GetPriceExtraKm(),
                policy.GetIsActive()
            );

            return Result<GetPolicyResponse>.Success(response);
        }
    }
}
