using OrdersMS.Core.Application.IdGenerator;
using OrdersMS.Core.Application.Services;
using OrdersMS.Core.Utils.Result;
using OrdersMS.src.Contracts.Application.Commands.CreateContract.Types;
using OrdersMS.src.Contracts.Application.Exceptions;
using OrdersMS.src.Contracts.Application.Repositories;
using OrdersMS.src.Contracts.Domain;
using OrdersMS.src.Contracts.Domain.ValueObjects;

namespace OrdersMS.src.Contracts.Application.Commands.CreateContract
{
    public class CreateContractCommandHandler(
        IContractRepository contractRepository,
        IInsuredVehicleRepository vehicleRepository,
        IPolicyRepository policyRepository,
        IdGenerator<string> idGenerator
    ) : IService<CreateContractCommand, CreateContractResponse>
    {
        private readonly IContractRepository _contractRepository = contractRepository;
        private readonly IInsuredVehicleRepository _vehicleRepository = vehicleRepository;
        private readonly IPolicyRepository _policyRepository = policyRepository;
        private readonly IdGenerator<string> _idGenerator = idGenerator;

        public async Task<Result<CreateContractResponse>> Execute(CreateContractCommand data)
        {
            var isContractExist = await _contractRepository.ContractExists(data.AssociatedPolicy, data.InsuredVehicle);
            if (isContractExist)
            {
                throw new ContractAlreadyExistException(data.AssociatedPolicy, data.InsuredVehicle);
            }

            var isPolicyExist = await _policyRepository.GetById(data.AssociatedPolicy);
            if (!isPolicyExist.HasValue)
            {
                throw new PolicyNotFoundException();
            }
            var isPolicyIsActive = await _policyRepository.IsActivePolicy(data.AssociatedPolicy);
            if (isPolicyIsActive == false)
            {
                throw new PolicyNotAvailableException();
            }

            var isVehicleExist = await _vehicleRepository.GetById(data.InsuredVehicle);
            if (!isVehicleExist.HasValue)
            {
                throw new VehicleNotFoundException();

            }
            var isVehicleIsActive = await _vehicleRepository.IsActiveVehicle(data.InsuredVehicle);
            if (isVehicleIsActive == false)
            {
                throw new VehicleNotAvailableException();
            }

            var id = _idGenerator.Generate();
            var contract = Contract.CreateContract(
                new ContractId(id),
                new PolicyId(data.AssociatedPolicy),
                new VehicleId(data.InsuredVehicle),
                new ContractStatus(data.Status)
            );
            await _contractRepository.Save(contract);

            return Result<CreateContractResponse>.Success(new CreateContractResponse(id));
        }
    }
}
