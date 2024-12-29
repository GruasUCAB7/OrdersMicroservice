namespace OrdersMS.src.Contracts.Application.Types
{
    public record GetContractResponse
    (
        string Id,
        int ContractNumber,
        string AssociatedPolicy,
        string InsuredVehicle,
        string StartDate,
        string ExpirationDate,
        string Status
    );
}
