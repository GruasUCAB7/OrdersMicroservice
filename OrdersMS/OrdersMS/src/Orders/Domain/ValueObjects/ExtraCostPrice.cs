using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.Orders.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.ValueObjects
{
    public class ExtraCostPrice : IValueObject<ExtraCostPrice>
    {
        public decimal Price { get; }

        public ExtraCostPrice(decimal price)
        {
            if (price < 0)
            {
                throw new InvalidExtraCostPriceException("Price must be a positive value.");
            }
            Price = price;
        }

        public decimal GetValue()
        {
            return Price;
        }

        public bool Equals(ExtraCostPrice other)
        {
            return Price == other.Price;
        }
    }
}
