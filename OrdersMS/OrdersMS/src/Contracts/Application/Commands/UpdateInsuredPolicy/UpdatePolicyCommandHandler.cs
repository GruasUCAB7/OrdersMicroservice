using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Commands.UpdateInsuredPolicy.Types;
using OrdersMS.src.Contracts.Application.Exceptions;
using OrdersMS.src.Contracts.Application.Repositories;

namespace OrdersMS.src.Contracts.Application.Commands.UpdateInsuredPolicy
{
    public class UpdatePolicyCommandHandler(IPolicyRepository policyRepository) : IService<(string id, UpdatePolicyCommand data), UpdatePolicyResponse>
    {
        private readonly IPolicyRepository _policyRepository = policyRepository;

        public async Task<Result<UpdatePolicyResponse>> Execute((string id, UpdatePolicyCommand data) request)
        {
            var policyOptional = await _policyRepository.GetById(request.id);
            if (!policyOptional.HasValue)
            {
                return Result<UpdatePolicyResponse>.Failure(new PolicyNotFoundException());
            }

            var policy = policyOptional.Unwrap();

            if (request.data.IsActive.HasValue)
            {
                policy.SetIsActive(request.data.IsActive.Value);
            }

            var updateResult = await _policyRepository.Update(policy);
            if (updateResult.IsFailure)
            {
                return Result<UpdatePolicyResponse>.Failure(new PolicyUpdateFailedException());
            }

            var response = new UpdatePolicyResponse(
                policy.GetId(),
                policy.GetPolicyType(),
                policy.GetPolicyCoverageKm(),
                policy.GetPolicyIncidentCoverageAmount(),
                policy.GetPriceExtraKm(),
                policy.GetIsActive()
            );

            return Result<UpdatePolicyResponse>.Success(response);
        }
    }
}
