namespace OrdersMS.src.Orders.Application.Commands.CreateOrder.Types
{
    public record CreateOrderCommand(
        string ContractId,
        string OperatorId,
        string? DriverAssigned,
        string IncidentAddress,
        string DestinationAddress,
        string IncidentType,
        List<string>? ExtraServicesApplied,
        string tokenJWT
     );
}
