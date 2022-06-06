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
            _serviceTask = new Task(LabelProcess);

            JobQueue.JobAdded += Run;
        }

        public void Run(object? sender, EventArgs e)
        {
            if (_serviceTask.IsCompleted || _serviceTask.Status == TaskStatus.Created)
            {
                _serviceTask.Start();
            }
        }

        private void LabelProcess()
        {
            do
            {
                var job = JobQueue.Next();

                if (job == null)
                    throw new NullReferenceException();

                var id = job.ShippingOrder.Id;

                _logger.LogDebug($"So there's this job... '{id}'");
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
