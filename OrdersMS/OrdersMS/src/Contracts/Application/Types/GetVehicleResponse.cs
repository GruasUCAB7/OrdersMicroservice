namespace OrdersMS.src.Contracts.Application.Types
{
    public record GetVehicleResponse
    (
        string Id,
        string Brand,
        string Model,
        string Plate,
        string VehicleSize,
        int Year,
        bool IsActive,
        string ClientDNI,
        string ClientName,
        string ClientEmail
    );
}
