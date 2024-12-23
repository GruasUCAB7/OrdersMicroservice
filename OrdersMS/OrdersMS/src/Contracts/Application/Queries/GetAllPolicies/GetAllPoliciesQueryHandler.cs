using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Exceptions;
using OrdersMS.src.Contracts.Application.Queries.GetAllPolicies.Types;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Application.Types;

namespace OrdersMS.src.Contracts.Application.Queries.GetAllPolicies
{
    public class GetAllPoliciesQueryHandler(IPolicyRepository policyRepository) : IService<GetAllPoliciesQuery, GetPolicyResponse[]>
    {
        private readonly IPolicyRepository _policyRepository = policyRepository;
        public async Task<Result<GetPolicyResponse[]>> Execute(GetAllPoliciesQuery data)
        {
            var policy = await _policyRepository.GetAll(data);
            if (policy == null)
            {
                return Result<GetPolicyResponse[]>.Failure(new PolicyNotFoundException());
            }

            var response = policy.Select(policy => new GetPolicyResponse(
                policy.GetId(),
                policy.GetPolicyType(),
                policy.GetPolicyCoverageKm(),
                policy.GetPolicyIncidentCoverageAmount(),
                policy.GetPriceExtraKm(),
                policy.GetIsActive()
                )
            ).ToArray();

            return Result<GetPolicyResponse[]>.Success(response);
        }
    }
}
