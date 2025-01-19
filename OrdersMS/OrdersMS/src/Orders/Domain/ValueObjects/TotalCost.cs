using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.Orders.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.ValueObjects
{
    public class TotalCost : IValueObject<TotalCost>
    {
        public decimal Total { get; }

        public TotalCost(decimal total)
        {
            if (total < -1)
            {
                throw new InvalidTotalCostException("Total cost must be a positive value.");
            }
            Total = total;
        }

        public decimal GetValue()
        {
            return Total;
        }

        public bool Equals(TotalCost other)
        {
            return Total == other.Total;
        }
    }
}
