using OrdersMS.Core.Domain.Exceptions;

namespace OrdersMS.src.Orders.Domain.Exceptions
{
    public class InvalidIncidentTypeException : DomainException
    {
        public InvalidIncidentTypeException(string type) : base(type)
        {
        }
    }
}
