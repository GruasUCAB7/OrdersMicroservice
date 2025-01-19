using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.Exceptions
{
    public class InvalidPolicyException : DomainException
    {
        public InvalidPolicyException() : base("Invalid insurance policy")
        {
        }
    }
}
