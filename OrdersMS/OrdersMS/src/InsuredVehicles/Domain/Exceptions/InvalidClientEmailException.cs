using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.InsuredVehicles.Domain.Exceptions
{
    public class InvalidClientEmailException : DomainException
    {
        public InvalidClientEmailException() : base("Invalid client email")
        {
        }
    }
}
