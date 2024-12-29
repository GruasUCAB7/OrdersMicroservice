using OrdersMS.Core.Domain.Events;
using OrdersMS.src.Contracts.Domain.ValueObjects;

namespace OrdersMS.src.Contracts.Domain.Events
{
    public class ContractCreatedEvent(string dispatcherId, string name, ContractCreated context) : DomainEvent<object>(dispatcherId, name, context) { }
    public class ContractCreated(string id, int number, string policyId, string vehicleId)
    {
        public readonly string Id = id;
        public readonly int ContractNumber = number;
        public readonly string PolicyId = policyId;
        public readonly string VehicleId =vehicleId;
        //public readonly string Status = status;

        public static ContractCreatedEvent CreateEvent(ContractId Id, ContractNumber ContractNumber, PolicyId PolicyId, VehicleId InsuredVehicle)
        {
            return new ContractCreatedEvent(
                Id.GetValue(),
                typeof(ContractCreated).Name,
                new ContractCreated(
                    Id.GetValue(),
                    ContractNumber.GetValue(),
                    PolicyId.GetValue(),
                    InsuredVehicle.GetValue()
                    //Status.GetValue()
                )
            );
        }

    }
}
