using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.Contracts.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.ValueObjects
{
    public class PriceExtraKm : IValueObject<PriceExtraKm>
    {
        private double Price { get; }

        public PriceExtraKm(double price)
        {
            if (price < 0 || price > 50)
            {
                throw new InvalidPriceExtraKmException();
            }
            Price = price;
        }

        public double GetValue()
        {
            return Price;
        }

        public bool Equals(PriceExtraKm other)
        {
            return Price == other.Price;
        }
    }
}
