namespace OrdersMS.src.Contracts.Application.Exceptions
{
    public class InsurancePolicyAlreadyExistException(string type) 
        : ApplicationException($"Insurance policy with type {type} already exist")
    {
    }
}
