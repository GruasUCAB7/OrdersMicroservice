using OrdersMS.Core.Domain.ValueObjects;
using OrdersMS.Core.Utils.RegExps;
using OrdersMS.src.Contracts.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.ValueObjects
{
    public class ClientDNI : IValueObject<ClientDNI>
    {
        private string DNI { get; }

        public ClientDNI(string dni)
        {
            if (!DNIRegex.IsMatch(dni))
            {
                throw new InvalidClientDNIException();
            }
            DNI = dni;
        }

        public string GetValue()
        {
            return DNI;
        }

        public bool Equals(ClientDNI other)
        {
            return DNI == other.DNI;
        }
    }
}
