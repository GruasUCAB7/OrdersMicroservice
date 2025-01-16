using OrdersMS.Core.Domain.Events;
using OrdersMS.src.Contracts.Domain.ValueObjects;
using OrdersMS.src.Orders.Domain.Entities;
using OrdersMS.src.Orders.Domain.ValueObjects;

namespace OrdersMS.src.Orders.Domain.Events
{
    public class OrderCreatedEvent(string dispatcherId, string name, OrderCreated context) : DomainEvent<object>(dispatcherId, name, context) { }
    public class OrderCreated(
        string id, 
        string contractId, 
        string operatorId,
        string driverId,
        Coordinates incidentAddress, 
        Coordinates destinationAddress,
        string incidentType,
        DateTime incidentDate,
        List<ExtraCost> extraServicesApplied
    )
    {
        public readonly string Id = id;
        public readonly string ContractId = contractId;
        public readonly string OperatorId = operatorId;
        public readonly string DriverId = driverId;
        public readonly Coordinates IncidentAddress = incidentAddress;
        public readonly Coordinates DestinationAddress = destinationAddress;
        public readonly string IncidentType = incidentType;
        public readonly DateTime IncidentDate = incidentDate;
        public readonly List<ExtraCost> ExtraServicesApplied = extraServicesApplied;

        public static OrderCreatedEvent CreateEvent(
            OrderId Id,
            ContractId ContractId, 
            UserId OperatorId,
            DriverId DriverId,
            Coordinates IncidentAddress, 
            Coordinates DestinationAddress,
            IncidentType IncidentType,
            DateTime IncidentDate,
            List<ExtraCost> ExtraServicesApplied)
        {
            return new OrderCreatedEvent(
                Id.GetValue(),
                typeof(OrderCreated).Name,
                new OrderCreated(
                    Id.GetValue(),
                    ContractId.GetValue(),
                    OperatorId.GetValue(),
                    DriverId.GetValue(),
                    IncidentAddress,
                    DestinationAddress,
                    IncidentType.GetValue(),
                    IncidentDate,
                    ExtraServicesApplied
                )
            );
        }

    }
}
