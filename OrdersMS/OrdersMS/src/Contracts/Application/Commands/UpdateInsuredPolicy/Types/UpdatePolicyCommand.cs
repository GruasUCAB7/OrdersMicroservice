namespace OrdersMS.src.Contracts.Application.Commands.UpdateInsuredPolicy.Types
{
    public record UpdatePolicyCommand
    (
        bool? IsActive,
        decimal? PolicyCoverageKm,
        decimal? PolicyIncidentCoverageAmount,
        decimal? PriceExtraKm
    );
}
