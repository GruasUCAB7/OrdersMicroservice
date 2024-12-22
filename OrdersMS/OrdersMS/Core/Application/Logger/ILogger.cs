namespace OrdersMS.Core.Application.Logger
{
    public interface ILoggerContract
    {
        void Log(params string[] data);
        void Error(params string[] data);
        void Exception(params string[] data);
    }
}
