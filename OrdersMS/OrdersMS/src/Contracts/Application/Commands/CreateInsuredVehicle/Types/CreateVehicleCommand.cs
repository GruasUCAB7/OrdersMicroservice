namespace OrdersMS.src.Contracts.Application.Commands.CreateInsuredVehicle.Types
{
    public record CreateVehicleCommand(
        string Brand,
        string Model,
        string Plate,
        string VehicleSize,
        int Year,
        string ClientDNI,
        string ClientName,
        string ClientEmail
    );
}
