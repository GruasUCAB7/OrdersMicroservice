namespace OrdersMS.src.Orders.Application.Exceptions
{
    public class OrderNotFoundException : ApplicationException
    {
        public OrderNotFoundException() : base("Order not found")
        {
        }
    }
}
