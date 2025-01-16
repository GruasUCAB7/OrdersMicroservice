using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.Exceptions
{
    public class InvalidContractYearException : DomainException
    {
        public InvalidContractYearException() : base("Invalid contract date")
        {
        }
    }
}
