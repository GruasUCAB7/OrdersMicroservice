namespace OrdersMS.src.Contracts.Application.Types
{
    public record GetPolicyResponse
    (
        string Id,
        string Type,
        decimal CoverageKm,
        decimal CoverageAmount,
        decimal PriceExtraKm,
        bool IsActive
    );
}
