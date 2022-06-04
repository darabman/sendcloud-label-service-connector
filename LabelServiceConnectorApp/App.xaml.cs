using System.Windows;
using Microsoft.Extensions.Logging;

namespace LabelServiceConnector
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            _logger = ConfigureLogger().CreateLogger("");
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            _logger.LogInformation(ResourceAssembly.GetName().Name + " started" );
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _logger.LogInformation(ResourceAssembly.GetName().Name + " exited");

            base.OnExit(e);
        }
    }
}
