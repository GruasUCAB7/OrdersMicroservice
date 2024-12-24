namespace OrdersMS.src.Contracts.Application.Types
{
    public record GetContractResponse
    (
        string Id,
        string AssociatedPolicy,
        string InsuredVehicle,
        string StartDate,
        string ExpirationDate,
        string Status
    );
}
