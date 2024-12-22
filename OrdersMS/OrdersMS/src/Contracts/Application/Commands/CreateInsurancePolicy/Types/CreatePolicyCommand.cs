namespace OrdersMS.src.Contracts.Application.Commands.CreateInsurancePolicy.Types
{
    public record CreatePolicyCommand(
        string Type,
        int CoverageKm,
        float CoverageAmount,
        float PriceExtraKm
    );
}
