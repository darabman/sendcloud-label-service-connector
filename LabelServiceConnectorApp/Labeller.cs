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
using Microsoft.Extensions.Configuration;

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
            #region Configure API

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

            var shippingMethods = webClient.GetShippingMethods().Result;

            #endregion

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

                var methodString = ConstructShippingMethodString(shippingMethods, job.ShippingOrder.Fields);
                
                var methodId = shippingMethods
                    .Where(m => m.Name == methodString)
                    .Select(m => m.Id)
                    .FirstOrDefault(-1);

                if (methodId == -1)
                {
                    _logger.LogError($"Unable to retrieve shipping method named '{methodString}', " 
                        + "please check the shipping order parameters}");

                    continue;
                }

                _logger.LogInformation($"Creating parcel with Shipping Method '{methodString}' ID [{methodId}] ");
                
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

        private string ConstructShippingMethodString(IEnumerable<ShippingMethod> methods, IDictionary<string, string> parameters)
        {

            var mode = parameters["mode_of_shipment"] ?? string.Empty;            
            var weight = float.Parse(parameters["weight"] ?? "0");

            _logger.LogDebug($"Shipping method is '{mode}' for parcel of weight '{weight}'");

            var mapping = Configuration.FieldMapping.GetSection(parameters["mode_of_shipment"] ?? "");

            var ranges = mapping
                .GetSection("WeightRanges")
                .GetChildren()
                .Select(w => new Tuple<int, int>(
                    int.Parse(w["min"]), 
                    int.Parse(w["max"])
                    ));

            //Select weight category by highest
            var range = ranges
                .Where(r => weight >= r.Item1 && weight <= r.Item2)
                .FirstOrDefault(new Tuple<int, int>(0, 100));
            
            return mapping["MethodString"]
                .Replace("{min}", $"{range.Item1}")
                .Replace("{max}", $"{range.Item2}");
        }
    }
}
