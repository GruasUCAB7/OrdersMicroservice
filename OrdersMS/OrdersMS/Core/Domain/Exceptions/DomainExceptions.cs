namespace OrdersMS.Core.Domain.Exceptions
{
    public abstract class DomainException(string message) : Exception(message)
    {
        public override string Message => base.Message;

        public static string ExceptionName => typeof(DomainException).Name;
    }
}
