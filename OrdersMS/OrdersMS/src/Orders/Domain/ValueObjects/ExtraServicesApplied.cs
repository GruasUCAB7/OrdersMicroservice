using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.Core.Utils.RegExps;
using OrdersMS.src.Orders.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.ValueObjects
{
    public class ExtraServicesApplied : IValueObject<ExtraServicesApplied>
    {
        public string Id { get; }
        public decimal Price { get; } = 0;

        public ExtraServicesApplied(string id, decimal price)
        {
            if (!UUIDRegExps.UUIDRegExp.IsMatch(id))
            {
                throw new InvalidExtraCostIdException();
            }

            if (price < 0)
            {
                throw new InvalidExtraCostPriceException("Price must be a positive value.");
            }

            Id = id;
            Price = price;
        }

        public string GetExtraCostId()
        {
            return Id;
        }

        public decimal GetExtraCostPrice()
        {
            return Price;
        }

        public bool Equals(ExtraServicesApplied other)
        {
            return Id == other.Id && Price == other.Price;
        }

        public override bool Equals(object obj)
        {
            if (obj is ExtraServicesApplied other)
            {
                return Equals(other);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Price);
        }
    }
}
