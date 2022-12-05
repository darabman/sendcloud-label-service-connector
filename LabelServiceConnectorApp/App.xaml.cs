using LabelServiceConnector.Agents;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Threading;
using System.Threading.Tasks;
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

        private readonly Archiver _archiverAgent;
        
        private readonly Loader _loaderAgent;

        public App()
        {
            _logger = ConfigureLogger().CreateLogger("");
            _cancellationToken = new CancellationToken();
            _notifyIcon = new Forms.NotifyIcon();

            _archiverAgent = new Archiver(_logger, _cancellationToken);
            _loaderAgent = new Loader(_logger, _cancellationToken);
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

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            BuildNotifyIcon();

            _logger.LogInformation(ResourceAssembly.GetName().Name + " started");

            new Labeller(_logger, _cancellationToken);

            _loaderAgent.Start();

            await _archiverAgent.UpdateDeliveredStatusKey();
            _archiverAgent.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _logger.LogInformation(ResourceAssembly.GetName().Name + " exited with code " + e.ApplicationExitCode);

            _notifyIcon.Dispose();

            base.OnExit(e);
        }
    }
}
