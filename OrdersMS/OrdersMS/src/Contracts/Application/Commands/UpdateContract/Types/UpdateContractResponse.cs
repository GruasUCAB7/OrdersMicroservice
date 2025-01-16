namespace OrdersMS.src.Contracts.Application.Commands.UpdateContract.Types
{
    public record UpdateContractResponse
    (
        string Id,
        string PolicyId,
        string VehicleId,
        DateTime StartDate,
        DateTime ExpirationDate,
        string Status
    );
}
