namespace OrdersMS.src.Orders.Application.Exceptions
{
    public class ExtraCostAlreadyExistException(string name)
        : ApplicationException($"Extra cost with name {name} already exist.")
    {
    }
}
