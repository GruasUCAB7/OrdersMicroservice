namespace OrdersMS.src.Contracts.Application.Commands.UpdateInsuredPolicy.Types
{
    public record UpdatePolicyResponse
    (
        string Id,
        string Type,
        int CoverageKm,
        double CoverageAmount,
        double PriceExtraKm,
        bool IsActive
    );
}
