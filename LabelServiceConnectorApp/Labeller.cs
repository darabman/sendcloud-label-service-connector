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
using System.Diagnostics;
using Microsoft.Win32;

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

                _logger.LogInformation($"Processing job '{job.Id}'");
                job.Status = Models.JobStatus.Fetching;

                var parcelRequests = new List<CreateParcel>();
                var badShippingMethod = false;
                var badMethodString = string.Empty;

                foreach (var order in job.ShippingOrders)
                {
                    try
                    {
                        //Build a new request from the shipping order's CSV fields
                        var jsonFields = JsonConvert.SerializeObject(order.Fields);
                        parcelRequests.Add(SendCloudApi.Net.Helpers.JsonHelper.Deserialize<CreateParcel>(jsonFields, ""));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Could not transform job '{job.Id}' into a valid request, skipping..");
                        _logger.LogDebug($"'{ex}' {ex.Message}");

                        break;
                    }

                    //Match shipping order data to appropriate Shipping Method
                    var methodString = ConstructShippingMethodString(shippingMethods, job.ShippingOrders[0].Fields);

                    var methodId = shippingMethods
                        .Where(m => m.Name == methodString)
                        .Select(m => m.Id)
                        .FirstOrDefault(-1);

                    if (methodId == -1)
                    {
                        badShippingMethod = true;
                        break;
                    }

                    _logger.LogInformation($"Creating parcel with Shipping Method '{methodString}' ID [{methodId}] ");

                    parcelRequests.Last().ColloCount = job.ShippingOrders.Count;                    
                    parcelRequests.Last().RequestLabel = true;
                    parcelRequests.Last().ShippingMethod = methodId;
                }

                if (badShippingMethod)
                {
                    _logger.LogError($"Found an unknown shipping method '{badMethodString}', "
                        + "please check the shipping order parameters");

                    continue;
                }

                if (parcelRequests.Count != job.ShippingOrders.Count)
                {
                    //Failed to transform all the SOs into valid requests
                    continue;
                }

                //Call the label provider and save the resulting PDF(s)
                var pdfOutputPaths = new List<string>();

                try
                {
                    var parcels = webClient.CreateParcel(parcelRequests).Result;

                    for (int i = 0; i < parcels.Length; i++)
                    {
                        job.ShippingOrders[i].TrackingNumber = parcels[i].TrackingNumber;
                    }

                    foreach (var parcel in parcels)
                    {
                        _logger.LogInformation($"Created parcel '{parcel.Id}' with tracking number '{parcel.TrackingNumber}'");
                        _logger.LogInformation($"Fetching label from '{parcel.Label.LabelPrinter}'");

                        var pdfBytes = webClient.DownloadLabel(parcel.Label.LabelPrinter).Result;

                        var outDir = Directory.CreateDirectory(Configuration.Config["PdfOutputDir"] ?? "./").FullName;
                        var pdfOutputPath = $"{outDir}{parcel.Id}.pdf";

                        _logger.LogInformation($"Saving label to '{pdfOutputPath}'");

                        using (var fw = File.OpenWrite(pdfOutputPath))
                        {
                            fw.Write(pdfBytes);
                        }

                        pdfOutputPaths.Add(pdfOutputPath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Unable to retrieve label(s)! '{ex}': {ex.Message}");

                    continue;
                }

                #endregion // Retrieve Label

                #region Write Back Tracking Number

                var outputDir = Configuration.Config["CsvOutputDir"] ?? "./";
                var fieldSep = Configuration.Config["CsvFieldSeparator"] ?? ";";

                var csvOut = Directory.CreateDirectory(outputDir) + job.Id + ".csv";

                _logger.LogInformation($"Saving updated shipping order to '{csvOut}'");

                using (var fw = File.CreateText(csvOut))
                {
                    foreach (var header in job.ShippingOrders[0].Fields.Keys)
                    {
                        fw.Write(header + fieldSep);
                    }

                    if (!job.ShippingOrders[0].Fields.ContainsKey("tracking_number"))
                    {
                        fw.Write("tracking_number");
                    }

                    foreach (var order in job.ShippingOrders)
                    {
                        fw.Write(Environment.NewLine);

                        foreach (var value in order.Fields.Values)
                        {
                            fw.Write(value + fieldSep);
                        }

                        if (!order.Fields.ContainsKey("tracking_number"))
                        {
                            fw.Write(order.TrackingNumber);
                        }
                        else
                        {
                            order.Fields["tracking_number"] = order.TrackingNumber ?? "";
                        }
                    }

                }

                #endregion // Write Back Tracking Number

                #region Print Label

                //Launch an installed printing app from command line, assuming Acrobat Reader
                var keyString = Configuration.Config["PrinterAppRegistryKey"] ??
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion" +
                    @"\App Paths\Acrobat.exe";
                var argString = Configuration.Config["PrinterAppArgumentString"] ?? "/h /t \"{0}\" \"{1}\"";
                var printerName = Configuration.Config["PrinterName"];

                var acrobatKey = Registry.LocalMachine.OpenSubKey(keyString);

                if (acrobatKey == null)
                {
                    _logger.LogWarning("Path to printer application was not recognised. Skipping printing..");
                    _logger.LogDebug($"PrinterAppRegistryKey: '{keyString}'");
                }
                else
                {
                    foreach (var path in pdfOutputPaths)
                    {
                        try
                        {
                            var proc = Process.Start(
                           fileName: $"{acrobatKey.GetValue("")}",
                           arguments: string.Format(argString, path, printerName));

                            _logger.LogInformation($"Printing '{path}'");
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("Couldn't execute printer application. Skipping printing..");
                            _logger.LogDebug($"'{ex}': {ex.Message}");
                        }
                    }
                }

                #endregion // Print Label

                _logger.LogDebug($"Finished processing job '{job.Id}'");

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
