using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.Exceptions
{
    public class InvalidPriceExtraKmException : DomainException
    {
        public InvalidPriceExtraKmException() : base("The price per extra kilometer must be a positive value.")
        {
        }
    }
}
