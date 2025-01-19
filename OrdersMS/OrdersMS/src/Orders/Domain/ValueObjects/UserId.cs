using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.Core.Utils.RegExps;
using OrdersMS.src.Orders.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.ValueObjects
{
    public class UserId : IValueObject<UserId>
    {
        private string Id { get; }

        public UserId(string id)
        {
            if (!UUIDRegExps.UUIDRegExp.IsMatch(id!) && id != "null")
            {
                throw new InvalidDriverIdException();
            }
            Id = id;
        }

        public string GetValue()
        {
            return Id;
        }

        public bool Equals(UserId other)
        {
            return Id == other.Id;
        }
    }
}
