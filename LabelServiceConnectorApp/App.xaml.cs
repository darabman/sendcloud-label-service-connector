﻿using Microsoft.Extensions.Logging;
using Serilog;
using System.Threading;
using System.Windows;

namespace LabelServiceConnector
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly Microsoft.Extensions.Logging.ILogger _logger;

        private CancellationToken _cancellationToken;

        public App()
        {
            _logger = ConfigureLogger().CreateLogger("");
            _cancellationToken = new CancellationToken();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _logger.LogInformation(ResourceAssembly.GetName().Name + " started" );

            new Loader(_logger, _cancellationToken).Run();
            new Labeller(_logger, _cancellationToken);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _logger.LogInformation(ResourceAssembly.GetName().Name + " exited with code " + e.ApplicationExitCode);

            base.OnExit(e);
        }

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
