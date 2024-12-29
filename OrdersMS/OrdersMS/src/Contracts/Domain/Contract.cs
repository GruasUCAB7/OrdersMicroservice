using OrdersMS.Core.Domain.Aggregates;
using OrdersMS.src.Contracts.Domain.Events;
using OrdersMS.src.Contracts.Domain.Exceptions;
using OrdersMS.src.Contracts.Domain.ValueObjects;

namespace OrdersMS.src.Contracts.Domain
{
    public class Contract(ContractId id) : AggregateRoot<ContractId>(id)
    {
        private ContractId _id = id;
        private ContractNumber _contractNumber;
        private PolicyId _policyId;
        private VehicleId _insuredVehicle;
        private DateOnly _startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        private DateOnly _expirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));
        private ContractStatus _status = new ContractStatus("Activo");

        public string GetId() => _id.GetValue();
        public int GetContractNumber() => _contractNumber.GetValue();
        public string GetPolicyId() => _policyId.GetValue();
        public string GetVehicleId() => _insuredVehicle.GetValue();
        public string GetStartDate() => _startDate.ToString();
        public string GetExpirationDate() => _expirationDate.ToString();
        public string GetStatus() => _status.GetValue();
        public void SetStatus(ContractStatus status) => _status = status;

        public static Contract CreateContract(ContractId id, ContractNumber number, PolicyId policyId, VehicleId insuredVehicle)
        {
            var contract = new Contract(id);
            contract.Apply(ContractCreated.CreateEvent(id, number, policyId, insuredVehicle));
            return contract;
        }

        public void OnContractCreatedEvent(ContractCreated context)
        {
            _id = new ContractId(context.Id);
            _contractNumber = new ContractNumber(context.ContractNumber);
            _policyId = new PolicyId(context.PolicyId);
            _insuredVehicle = new VehicleId(context.VehicleId);
            _startDate = DateOnly.FromDateTime(DateTime.UtcNow);
            _expirationDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));
        }

        public override void ValidateState()
        {
            if (_id == null || _contractNumber == null || _policyId == null || _insuredVehicle == null || _status == null)
            {
                throw new InvalidContractException();
            }
        }
    }
}
