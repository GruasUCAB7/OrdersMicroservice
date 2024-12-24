using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.Core.Utils.RegExps;
using OrdersMS.src.Orders.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.ValueObjects
{
    public class ExtraCostId : IValueObject<ExtraCostId>
    {
        private string Id { get; }

        public ExtraCostId(string id)
        {
            if (!UUIDRegExps.UUIDRegExp.IsMatch(id))
            {
                throw new InvalidExtraCostIdException();
            }
            Id = id;
        }

        public string GetValue()
        {
            return Id;
        }

        public bool Equals(ExtraCostId other)
        {
            return Id == other.Id;
        }
    }
}
