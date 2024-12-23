using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.src.InsuredVehicles.Domain.Exceptions;

namespace OrdersMS.src.InsuredVehicles.Domain.ValueObjects
{
    public class ClientName : IValueObject<ClientName>
    {
        private string Name { get; }

        public ClientName(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length < 2)
            {
                throw new InvalidClientNameException();
            }
            Name = name;
        }

        public string GetValue()
        {
            return Name;
        }

        public bool Equals(ClientName other)
        {
            return Name == other.Name;
        }
    }
}
