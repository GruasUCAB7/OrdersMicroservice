namespace OrdersMS.Core.Application.Exceptions
{
    public class ApplicationException(string message) : Exception(message)
    {
        private readonly string _message = message;
        public override string Message => _message;
    }
}
