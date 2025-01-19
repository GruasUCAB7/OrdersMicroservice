using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.Exceptions
{
    public class InvalidPolicyIdException : DomainException
    {
        public InvalidPolicyIdException() : base("Invalid policy ID")
        {
        }
    }
}
