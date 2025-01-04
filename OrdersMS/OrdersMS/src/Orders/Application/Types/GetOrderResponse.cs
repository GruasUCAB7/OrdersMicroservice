namespace OrdersMS.src.Orders.Application.Types
{
    public record GetOrderResponse(
    string Id,
    string ContractClient,
    string DriverAssigned,
    CoordinatesDto IncidentAddress,
    CoordinatesDto DestinationAddress,
    string IncidentDate,
    List<ExtraServiceDto> ExtraServicesApplied,
    decimal TotalCost,
    string Status
);

    public record CoordinatesDto(
        double Latitude,
        double Longitude
    );

    public record ExtraServiceDto(
        string Id,
        string Name,
        decimal Price
    );
}