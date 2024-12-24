using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Contracts.Domain.Exceptions
{
    public class InvalidContractIdException : DomainException
    {
        public InvalidContractIdException() : base("Invalid contract ID")
        {
        }
    }
}
