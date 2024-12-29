using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.Core.Utils.RegExps;
using OrdersMS.src.Orders.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.ValueObjects
{
    public class OrderId : IValueObject<OrderId>
    {
        private string Id { get; }

        public OrderId(string id)
        {
            if (!UUIDRegExps.UUIDRegExp.IsMatch(id))
            {
                throw new InvalidOrderIdException();
            }
            Id = id;
        }

        public string GetValue()
        {
            return Id;
        }

        public bool Equals(OrderId other)
        {
            return Id == other.Id;
        }
    }
}
