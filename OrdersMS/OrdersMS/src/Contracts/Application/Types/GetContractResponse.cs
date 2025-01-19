namespace OrdersMS.src.Contracts.Application.Types
{
    public record GetContractResponse
    (
        string Id,
        int ContractNumber,
        string AssociatedPolicy,
        string InsuredVehicle,
        DateTime StartDate,
        DateTime ExpirationDate,
        string Status
    );
}
