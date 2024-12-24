using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.Orders.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.ValueObjects
{
    public class ExtraCostName : IValueObject<ExtraCostName>
    {
        private string Name { get; }

        public ExtraCostName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
            {
                throw new InvalidExtraCostNameException();
            }
            Name = name;
        }

        public string GetValue()
        {
            return Name;
        }

        public bool Equals(ExtraCostName other)
        {
            return Name == other.Name;
        }
    }
}
