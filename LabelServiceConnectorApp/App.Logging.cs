using Microsoft.Extensions.Logging;
using Serilog;


namespace LabelServiceConnector
{
    public partial class App
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        private ILoggerFactory ConfigureLogger()
        {
            return LoggerFactory.Create(builder =>
            {
                var config = new LoggerConfiguration()
                .WriteTo.Debug()
                .WriteTo.File("logs/lsca-.log", rollingInterval: RollingInterval.Day);

                builder.AddSerilog(config.CreateLogger());
            });
        }
    }
}
