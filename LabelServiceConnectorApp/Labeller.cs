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
using System.Globalization;

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

            var ep = Configuration.Api["EndPoint"];
            var key = Configuration.Api["ApiKey"];
            var secret = Configuration.Api["ApiSecret"];

            _logger.LogDebug($"Constructing API with " +
                $"Endpoint '{ep}' " +
                $"Key '{key}', " +
                $"Secret '{secret[0] + new string('*', secret.Length - 2) + secret[^1]}'");

            IWebClient webClient = (ep == "None")
                ? new EmptyWebClient()
                : new SendCloudWebClient(ep, key, secret);
            
            //List of shipping methods won't change between jobs so retrieve only once
            ShippingMethod[] shippingMethods;

            try
            {
                shippingMethods = webClient.GetShippingMethods().Result;
            }
            catch (Exception ex)
            {
                _logger.LogError("Couldn't get shipping methods from Label Provider!");
                _logger.LogDebug($"'{ex}' {ex.Message}");

                return;
            }            

            #endregion // Configure API

            do
            {
                #region Retrieve Label

                var job = JobQueue.Next();

                if (job == null)
                {
                    _logger.LogWarning("Job queue returned null");                    
                    break;
                }

                _logger.LogInformation($"Processing job '{job.ShippingOrder.Id}'");
                job.Status = Models.JobStatus.Fetching;

                //Build a new request from the shipping order's CSV fields
                CreateParcel request;

                try
                {
                    var jsonFields = JsonConvert.SerializeObject(job.ShippingOrder.Fields);
                    request = SendCloudApi.Net.Helpers.JsonHelper.Deserialize<CreateParcel>(jsonFields, "");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Could not transform job '{job.ShippingOrder.Id}' into a valid request, skipping..");
                    _logger.LogDebug($"'{ex}' {ex.Message}");

                    continue;
                }

                //Match shipping order data to appropriate Shipping Method
                var methodString = ConstructShippingMethodString(shippingMethods, job.ShippingOrder.Fields);
                
                var methodId = shippingMethods
                    .Where(m => m.Name == methodString)
                    .Select(m => m.Id)
                    .FirstOrDefault(-1);

                if (methodId == -1)
                {
                    _logger.LogError($"Unable to find a shipping method named '{methodString}', " 
                        + "please check the shipping order parameters");

                    continue;
                }

                _logger.LogInformation($"Creating parcel with Shipping Method '{methodString}' ID [{methodId}] ");

                request.RequestLabel = true;
                request.ShippingMethod = methodId;

                //Call the label provider and save the resulting PDF
                try
                {
                    var parcel = webClient.CreateParcel(request).Result;

                    job.ShippingOrder.TrackingNumber = parcel.TrackingNumber;

                    _logger.LogInformation($"Created parcel '{parcel.Id}' with tracking number '{parcel.TrackingNumber}'");
                    _logger.LogInformation($"Fetching label from '{parcel.Label.LabelPrinter}'");

                    var pdfBytes = webClient.DownloadLabel(parcel.Label.LabelPrinter).Result;

                    var outDir = Directory.CreateDirectory(Configuration.Config["PdfOutputDir"] ?? "./");
                    var outPath = $"{outDir}{parcel.Id}.pdf";

                    _logger.LogInformation($"Saving label to '{outPath}'");

                    File.WriteAllBytes(outPath, pdfBytes);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Unable to retrieve label! '{ex}': {ex.Message}");

                    continue;
                }

                #endregion // Retrieve Label

                #region Write Back Tracking Number

                var outputDir = Configuration.Config["CsvOutputDir"] ?? "./";
                var fieldSep = Configuration.Config["CsvFieldSeparator"] ?? ";";

                var csvOut = Directory.CreateDirectory(outputDir) + job.ShippingOrder.Id + ".csv";

                _logger.LogInformation($"Saving updated shipping order to '{csvOut}'");

                using (var fw = File.CreateText(csvOut))
                {
                    foreach (var header in job.ShippingOrder.Fields.Keys)
                    {
                        fw.Write(header + fieldSep);
                    }

                    if (!job.ShippingOrder.Fields.ContainsKey("tracking_number"))
                    {
                        fw.Write("tracking_number");
                    }

                    fw.Write(Environment.NewLine);

                    foreach (var value in job.ShippingOrder.Fields.Values)
                    {
                        fw.Write(value + fieldSep);
                    }

                    if (!job.ShippingOrder.Fields.ContainsKey("tracking_number"))
                    {
                        fw.Write(job.ShippingOrder.TrackingNumber);
                    }
                    else
                    {
                        job.ShippingOrder.Fields["tracking_number"] = job.ShippingOrder.TrackingNumber;
                    }                        
                } 

                #endregion // Write Back Tracking Number

                #region Print Label

                #endregion // Print Label

                _logger.LogDebug($"Finishing processing job '{job.ShippingOrder.Id}'");

            } while (JobQueue.JobReady);

            _logger.LogInformation("Job Queue Empty");
        }

        private string ConstructShippingMethodString(IEnumerable<ShippingMethod> methods, IDictionary<string, string> parameters)
        {
            parameters.TryGetValue("mode_of_shipment", out string? mode);

            if (string.IsNullOrEmpty(mode))
            {
                _logger.LogWarning("The mode was not defined in the Shipping Order, using default");
                mode = "default";
            }

            if (!parameters.TryGetValue("weight", out string? weightString) ||
                !float.TryParse(weightString, NumberStyles.Any, CultureInfo.InvariantCulture, out float weight))
            {
                _logger.LogError("No value specified for parcel weight!");
                return "no_weight";
            }

            _logger.LogDebug($"Shipping method is '{mode}' for parcel of weight '{weight}'");

            var mapping = Configuration.FieldMapping.GetSection(mode);
            
            var methodString = mapping["MethodString"];
            var ranges = mapping
                .GetSection("WeightRanges")
                .GetChildren()
                .Select(w => new Tuple<int, int>(
                    int.Parse(w["min"]), 
                    int.Parse(w["max"])
                    ));

            //Select weight category by highest
            var range = ranges
                .Where(r => weight >= r.Item1 && weight < r.Item2)
                .OrderByDescending(r => r.Item2)
                .FirstOrDefault(new Tuple<int, int>(0, 100));

            if (range != default)
            {
                //Interpolate range values, if they exist
                methodString = methodString
                                .Replace("{min}", $"{range.Item1}")
                                .Replace("{max}", $"{range.Item2}");
            }

            return methodString;
        }
    }
}
