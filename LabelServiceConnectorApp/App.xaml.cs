using Microsoft.Extensions.Logging;
using Serilog;
using System.Threading;
using System.Windows;
using Forms = System.Windows.Forms;

namespace LabelServiceConnector
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        private CancellationToken _cancellationToken;

        private readonly Forms.NotifyIcon _notifyIcon;

        public App()
        {
            _logger = ConfigureLogger().CreateLogger("");
            _cancellationToken = new CancellationToken();
            _notifyIcon = new Forms.NotifyIcon();
        }

        private ILoggerFactory ConfigureLogger()
        {
            return LoggerFactory.Create(builder =>
            {
                var config = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration.Logging);

                builder.AddSerilog(config.CreateLogger(), dispose: true);
            });
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            BuildNotifyIcon();

            _logger.LogInformation(ResourceAssembly.GetName().Name + " started");

            new Labeller(_logger, _cancellationToken);

            new Loader(_logger, _cancellationToken).Start();
            new Archiver(_logger, _cancellationToken).Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _logger.LogInformation(ResourceAssembly.GetName().Name + " exited with code " + e.ApplicationExitCode);

            _notifyIcon.Dispose();

            base.OnExit(e);
        }
    }
}
