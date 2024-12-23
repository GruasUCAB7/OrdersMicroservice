namespace OrdersMS.src.Contracts.Application.Types
{
    public record GetPolicyResponse
    (
        string Id,
        string Type,
        int CoverageKm,
        double CoverageAmount,
        double PriceExtraKm,
        bool IsActive
    );
}
