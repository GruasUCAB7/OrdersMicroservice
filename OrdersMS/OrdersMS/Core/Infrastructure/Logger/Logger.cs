using OrdersMS.Core.Application.Logger;

namespace OrdersMS.Core.Infrastructure.Logger
{
    public class Logger : ILoggerContract
    {
        private readonly ILogger<Logger> _logger;

        public Logger(ILogger<Logger> logger)
        {
            _logger = logger;
        }

        public void Log(params string[] data)
        {
            _logger.LogInformation("{Message}", string.Join(" ", data));
        }

        public void Error(params string[] data)
        {
            _logger.LogError("ERROR: {Message}", string.Join(" ", data));
        }

        public void Exception(params string[] data)
        {
            _logger.LogError("EXCEPTION: {Message}", string.Join(" ", data));
        }
    }
}
