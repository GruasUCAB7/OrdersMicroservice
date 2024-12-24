using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Commands.UpdateContract.Types;
using OrdersMS.src.Contracts.Application.Exceptions;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain.ValueObjects;

namespace OrdersMS.src.Contracts.Application.Commands.UpdateContract
{
    public class UpdateContractCommandHandler(IContractRepository contractRepository) 
        : IService<(string id, UpdateContractCommand data), UpdateContractResponse>
    {
        private readonly IContractRepository _contractRepository = contractRepository;

        public async Task<Result<UpdateContractResponse>> Execute((string id, UpdateContractCommand data) request)
        {
            var contractOptional = await _contractRepository.GetById(request.id);
            if (!contractOptional.HasValue)
            {
                return Result<UpdateContractResponse>.Failure(new ContractNotFoundException());
            }

            var contract = contractOptional.Unwrap();

            if (!string.IsNullOrEmpty(request.data.Status))
            {
                var status = new ContractStatus(request.data.Status);
                contract.SetStatus(status);
            }

            var updateResult = await _contractRepository.Update(contract);
            if (updateResult.IsFailure)
            {
                return Result<UpdateContractResponse>.Failure(new ContractUpdateFailedException());
            }

            var response = new UpdateContractResponse(
                contract.GetId(),
                contract.GetPolicyId(),
                contract.GetVehicleId(),
                contract.GetStartDate(),
                contract.GetExpirationDate(),
                contract.GetStatus()
            );

            return Result<UpdateContractResponse>.Success(response);
        }
    }
}
