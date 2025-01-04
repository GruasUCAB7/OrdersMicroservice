using OrdersMS.Core.Domain.Services;
using OrdersMS.src.Orders.Domain.Services.Types;
using OrdersMS.src.Orders.Domain.ValueObjects;

namespace OrdersMS.src.Orders.Domain.Services
{
    public class CalculateOrderTotalAmount : IDomainService<CalculateOrderTotalAmountInput, TotalCost>
    {
        public TotalCost Execute(CalculateOrderTotalAmountInput data)
        {
            decimal totalAmount = 0;

            if (data.TotalKmTraveled > data.PolicyKmCoverage)
            {
                decimal extraKm = data.TotalKmTraveled - data.PolicyKmCoverage;
                totalAmount = totalAmount + (extraKm * data.ExtraKmPrice);
            }

            if (data.ExtraCostPrices != null)
            {
                totalAmount += data.ExtraCostPrices.Sum();
            }

            if (totalAmount >= data.PolicyAmountCoverage)
            {
                totalAmount = totalAmount - data.PolicyAmountCoverage;
            }
            else if (totalAmount < data.PolicyAmountCoverage)
            {
                totalAmount = 0;
            }

            return new TotalCost(totalAmount);
        }
    }
}
