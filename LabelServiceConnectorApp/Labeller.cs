using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using LabelServiceConnector.WebApi;
using KeyEncryptorLib;
using SendCloudApi.Net.Models;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

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
            //Load API configuration
            var ep = Configuration.Api["EndPoint"];
            var key = Configuration.Api["ApiKey"];
            var cryptedSecret = Configuration.Api["EncryptedSecret"];
            string secret = string.Empty;

            try
            {
                secret = KeyEncryptor.Decrypt(cryptedSecret);
            }
            catch (Exception ex)
            {
                if (ep != "None")
                {
                    _logger.LogError("Could not decrypt API secret from application settings");
                    _logger.LogDebug(ex.ToString() + $" {ex.Message}");

                    return;
                }
            }

            _logger.LogDebug($"Constructing API with " +
                $"Endpoint '{ep}' " +
                $"Key '{key}', " +
                $"Secret '{secret[0] + new string('*', secret.Length - 2) + secret[^1]}'");

            IWebClient webClient = (ep == "None")
                ? new EmptyWebClient()
                : new SendCloudWebClient(ep, key, secret);

            do
            {
                var job = JobQueue.Next();

                if (job == null)
                {
                    _logger.LogWarning("Job queue returned null");                    
                    break;
                }

                _logger.LogInformation($"Processing job '{job.ShippingOrder.Id}'");
                job.Status = Models.JobStatus.Fetching;

                //Build a new request from job
                CreateParcel request;

                try
                {
                    var jsonFields = JsonConvert.SerializeObject(job.ShippingOrder.Fields);
                    request = SendCloudApi.Net.Helpers.JsonHelper.Deserialize<CreateParcel>(jsonFields, "");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Could not transform job '{job.ShippingOrder.Id}' into a valid request, skipping..");
                    _logger.LogDebug(ex.ToString() + $" {ex.Message}");

                    continue;
                }

                var methodName = "Unstamped letter";
                var methods = webClient.GetShippingMethods().Result;
                var methodId = methods
                    .Where(m => m.Name == methodName)
                    .Select(m => m.Id)
                    .FirstOrDefault(-1);

                _logger.LogInformation($"Creating parcel with Shipping Method ID [{methodId}] '{methodName}' ");
                
                request.RequestLabel = true;
                request.ShippingMethod = methodId;

                var parcel = webClient.CreateParcel(request).Result;

                _logger.LogInformation($"Created parcel '{parcel.Id}'");
                _logger.LogInformation($"Fetching label from '{parcel.Label.LabelPrinter}'");

                var pdfBytes = webClient.DownloadLabel(parcel.Label.LabelPrinter).Result;
                var pdfPath = $"pdf_out/{parcel.Id}.pdf";

                _logger.LogInformation($"Saving label to '{pdfPath}'");

                File.WriteAllBytes(pdfPath, pdfBytes);

                //_logger.LogDebug($"Now I'm Sending it to the printer... '{id}'");
                Thread.Sleep(1200);

                //_logger.LogDebug($"Now I'm writing it back to disk... '{id}'");
                Thread.Sleep(500);

                _logger.LogDebug("Done!");

            } while (JobQueue.JobReady);

            _logger.LogInformation("Job Queue Empty");
        }
    }
}
