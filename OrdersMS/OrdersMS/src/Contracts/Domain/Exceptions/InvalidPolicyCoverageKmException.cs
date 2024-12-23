using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.Exceptions
{
    public class InvalidPolicyCoverageKmException : DomainException
    {
        public InvalidPolicyCoverageKmException() : base("Coverage per Km must be between 1 and 1,000,000.")
        {
        }
    }
}
