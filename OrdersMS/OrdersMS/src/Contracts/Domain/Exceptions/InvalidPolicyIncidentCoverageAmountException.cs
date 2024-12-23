using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.Exceptions
{
    public class InvalidPolicyIncidentCoverageAmountException : DomainException
    {
        public InvalidPolicyIncidentCoverageAmountException() : base("Coverage amount per incident must be a positive value.")
        {
        }
    }
}
