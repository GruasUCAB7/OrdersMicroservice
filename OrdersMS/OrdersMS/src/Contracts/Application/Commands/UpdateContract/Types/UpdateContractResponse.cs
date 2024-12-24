namespace OrdersMS.src.Contracts.Application.Commands.UpdateContract.Types
{
    public record UpdateContractResponse
    (
        string Id,
        string PolicyId,
        string VehicleId,
        string StartDate,
        string ExpirationDate,
        string Status
    );
}
