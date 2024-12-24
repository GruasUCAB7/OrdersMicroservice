namespace OrdersMS.src.Orders.Application.Exceptions
{
    public class ExtraCostUpdateFailedException : ApplicationException
    {
        public ExtraCostUpdateFailedException() : base("The extra cost could not be updated correctly")
        {
        }
    }
}
