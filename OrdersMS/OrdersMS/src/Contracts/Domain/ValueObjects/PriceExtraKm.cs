using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.Contracts.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.ValueObjects
{
    public class PriceExtraKm : IValueObject<PriceExtraKm>
    {
        private decimal Price { get; }

        public PriceExtraKm(decimal price)
        {
            if (price < 0 || price > 50)
            {
                throw new InvalidPriceExtraKmException();
            }
            Price = price;
        }

        public decimal GetValue()
        {
            return Price;
        }

        public bool Equals(PriceExtraKm other)
        {
            return Price == other.Price;
        }
    }
}
