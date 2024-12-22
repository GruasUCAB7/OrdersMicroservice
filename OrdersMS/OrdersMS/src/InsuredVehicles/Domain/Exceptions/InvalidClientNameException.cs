using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.InsuredVehicles.Domain.Exceptions
{
    public class InvalidClientNameException : DomainException
    {
        public InvalidClientNameException() : base("Invalid client name")
        {
        }
    }
}
