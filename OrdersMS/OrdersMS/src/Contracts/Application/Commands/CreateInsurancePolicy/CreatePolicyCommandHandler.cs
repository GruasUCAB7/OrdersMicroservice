using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Commands.CreateInsurancePolicy.Types;
using OrdersMS.src.Contracts.Application.Exceptions;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain.Entities;
using OrdersMS.src.Contracts.Domain.ValueObjects;

namespace OrdersMS.src.Contracts.Application.Commands.CreateInsurancePolicy
{
    public class CreatePolicyCommandHandler(
        IPolicyRepository policyRepository,
        IdGenerator<string> idGenerator
    ) : IService<CreatePolicyCommand, CreatePolicyResponse>
    {
        private readonly IPolicyRepository _policyRepository = policyRepository;
        private readonly IdGenerator<string> _idGenerator = idGenerator;

        public async Task<Result<CreatePolicyResponse>> Execute(CreatePolicyCommand data)
        {
            var isPolicyExist = await _policyRepository.ExistByType(data.Type);
            if (isPolicyExist)
            {
                return Result<CreatePolicyResponse>.Failure(new InsurancePolicyAlreadyExistException(data.Type));
            }

            var id = _idGenerator.Generate();
            var policy = new InsurancePolicy(
                new PolicyId(id), 
                new PolicyType(data.Type), 
                new PolicyCoverageKm(data.CoverageKm),
                new PolicyIncidentCoverageAmount(data.CoverageAmount),
                new PriceExtraKm(data.PriceExtraKm)
            ); 
            await _policyRepository.Save(policy);

            return Result<CreatePolicyResponse>.Success(new CreatePolicyResponse(id));
        }
    }
}
