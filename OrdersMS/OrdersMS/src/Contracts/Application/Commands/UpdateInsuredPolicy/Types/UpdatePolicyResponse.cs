namespace OrdersMS.src.Contracts.Application.Commands.UpdateInsuredPolicy.Types
{
    public record UpdatePolicyResponse
    (
        string Id,
        string Type,
        decimal CoverageKm,
        decimal CoverageAmount,
        decimal PriceExtraKm,
        bool IsActive
    );
}
