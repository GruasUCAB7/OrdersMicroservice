namespace OrdersMS.src.Orders.Domain.Services.Types
{
    public record CalculateOrderTotalAmountInput(
        decimal TotalKmTraveled,
        decimal PolicyKmCoverage,
        decimal PolicyAmountCoverage,
        decimal ExtraKmPrice,
        List<decimal> ExtraCostPrices
    );
}
