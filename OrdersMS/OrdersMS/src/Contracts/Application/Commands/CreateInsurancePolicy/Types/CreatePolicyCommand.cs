namespace OrdersMS.src.Contracts.Application.Commands.CreateInsurancePolicy.Types
{
    public record CreatePolicyCommand(
        string Type,
        decimal CoverageKm,
        decimal CoverageAmount,
        decimal PriceExtraKm
    );
}
