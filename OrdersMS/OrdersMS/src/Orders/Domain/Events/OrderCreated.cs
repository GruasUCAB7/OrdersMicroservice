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
        string? driverId,
        Coordinates incidentAddress, 
        Coordinates destinationAddress,
        List<ExtraCost> extraServicesApplied
    )
    {
        public readonly string Id = id;
        public readonly string ContractId = contractId;
        public readonly string? DriverId = driverId;
        public readonly Coordinates IncidentAddress = incidentAddress;
        public readonly Coordinates DestinationAddress = destinationAddress;
        public readonly List<ExtraCost> ExtraServicesApplied = extraServicesApplied;

        public static OrderCreatedEvent CreateEvent(
            OrderId Id,
            ContractId ContractId, 
            DriverId? DriverId,
            Coordinates IncidentAddress, 
            Coordinates DestinationAddress,
            List<ExtraCost> ExtraServicesApplied)
        {
            return new OrderCreatedEvent(
                Id.GetValue(),
                typeof(OrderCreated).Name,
                new OrderCreated(
                    Id.GetValue(),
                    ContractId.GetValue(),
                    DriverId?.GetValue(),
                    IncidentAddress,
                    DestinationAddress,
                    ExtraServicesApplied
                )
            );
        }

    }
}
