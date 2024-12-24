namespace OrdersMS.src.Orders.Application.Exceptions
{
    public class ExtraCostNotFoundException : ApplicationException
    {
        public ExtraCostNotFoundException() : base("Extra cost not found")
        {
        }
    }
}
