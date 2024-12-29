using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.Orders.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.ValueObjects
{
    public class TotalCost : IValueObject<TotalCost>
    {
        public double Total { get; }

        public TotalCost(double total)
        {
            if (total < -1)
            {
                throw new InvalidTotalCostException("Total cost must be a positive value.");
            }
            Total = total;
        }

        public double GetValue()
        {
            return Total;
        }

        public bool Equals(TotalCost other)
        {
            return Total == other.Total;
        }
    }
}
