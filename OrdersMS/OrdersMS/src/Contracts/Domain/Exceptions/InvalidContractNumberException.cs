using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.Exceptions
{
    public class InvalidContractNumberException : DomainException
    {
        public InvalidContractNumberException() : base("Invalid contract number")
        {
        }
    }
}
