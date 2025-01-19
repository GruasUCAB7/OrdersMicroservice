using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.Core.Utils.RegExps;
using OrdersMS.src.Orders.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.ValueObjects
{
    public class DriverId : IValueObject<DriverId>
    {
        private string Id { get; }

        public DriverId(string id)
        {
            if (!UUIDRegExps.UUIDRegExp.IsMatch(id!) && id != "Por asignar")
            {
                throw new InvalidDriverIdException();
            }
            Id = id;
        }

        public string GetValue()
        {
            return Id;
        }

        public bool Equals(DriverId other)
        {
            return Id == other.Id;
        }
    }
}
