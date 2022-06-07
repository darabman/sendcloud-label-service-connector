using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace LabelServiceConnector
{
    public class Labeller
    {
        private ILogger _logger;

        private Task _serviceTask;

        private CancellationToken _cancel;

        public Labeller(ILogger logger, CancellationToken cancel)
        {
            _cancel = cancel;
            _logger = logger;
            _serviceTask = Task.CompletedTask;

            JobQueue.JobAdded += Run;
        }

        public void Run(object? sender, EventArgs e)
        {
            if (_serviceTask.IsCompleted)
            {
                _serviceTask = Task.Run(LabelProcess);
            }
        }

        private void LabelProcess()
        {
            do
            {
                var job = JobQueue.Next();

                if (job == null)
                {
                    _logger.LogWarning("Job queue returned null");                    
                    break;
                }

                var id = job.ShippingOrder.Id;

                _logger.LogInformation($"Processing job '{id}'");
                Thread.Sleep(2500);

                _logger.LogDebug($"Now I'm Sending it to the printer... '{id}'");
                Thread.Sleep(1200);

                _logger.LogDebug($"Now I'm writing it back to disk... '{id}'");
                Thread.Sleep(500);

                _logger.LogDebug("Done!");

            } while (JobQueue.JobReady);

            _logger.LogDebug("Job Queue Empty");
        }
    }
}
