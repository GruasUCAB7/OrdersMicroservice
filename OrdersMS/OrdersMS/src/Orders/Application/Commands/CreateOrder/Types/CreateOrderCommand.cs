namespace OrdersMS.src.Orders.Application.Commands.CreateOrder.Types
{
    public record CreateOrderCommand(
        string ContractId,
        string? DriverAssigned,
        string IncidentAddress,
        string DestinationAddress,
        List<string>? ExtraServicesApplied
     );
}
