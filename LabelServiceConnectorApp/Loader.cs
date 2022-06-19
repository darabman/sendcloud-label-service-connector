using LabelServiceConnector.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;

namespace LabelServiceConnector
{
    internal class Loader
    {
        private CancellationToken _cancel;

        private ILogger _logger;

        public Loader(ILogger logger, CancellationToken cancel)
        {
            _cancel = cancel;           
            _logger = logger;
        }

        public void Run()
        {
            new Task(() =>
            {
                var delayMs = int.Parse(Configuration.Config["CsvScanRateMs"] ?? "1000");
                var path = Directory.CreateDirectory(Configuration.Config["CsvInputDir"] ?? "./");
                var encoding = Encoding.GetEncoding(Configuration.Config["TextEncoding"] ?? "iso-8859-1");

                while (!_cancel.IsCancellationRequested)
                {
                    try
                    {
                        ScanDirectory(path, encoding);
                        Thread.Sleep(delayMs);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Could not read CSVs from directory '{path}'");
                        _logger.LogDebug(ex.ToString() + $" {ex.Message}");

                        break;
                    }
                }

            }).Start();
        }

        public void ScanDirectory(DirectoryInfo dir, Encoding fileEncoding)
        {
            var shippingOrders = new List<ShippingOrder>();
            var files = dir.GetFiles("*.csv").OrderBy(f => f.CreationTime);            

            if (files.Any())
                _logger.LogInformation($"Found {files.Count()} CSV files in '{dir}'");

            foreach (var file in files)
            {
                try
                {
                    var orders = new List<ShippingOrder>();

                    using (var fileText = new StreamReader(file.FullName, fileEncoding))
                    {
                        orders = ParseCSV(fileText.ReadToEnd());
                    }

                    _logger.LogInformation($"Retrieved {orders.Count} order(s) from '{file.Name}'");

                    JobQueue.AddJob(new Job(orders, file));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Unable to process '{file.Name}', skipping..");
                    _logger.LogDebug($"{ex}: {ex.Message}");

                    Directory.CreateDirectory(dir + "/error/");
                    file.CopyTo(dir + "/error/" + file.Name, overwrite: true);
                }
                finally
                {
                    file.Delete();
                }
            }
        }

        private List<ShippingOrder> ParseCSV(string text)
        {
            var fieldSep = Configuration.Config["CsvFieldSeparator"] ?? ";";

            var rows = text.Split(Environment.NewLine);
            var header = rows[0].Split(fieldSep);
            var orders = new List<ShippingOrder>();
            var inconsistentRows = false;

            foreach (var row in rows.Skip(1).Where(str => !string.IsNullOrEmpty(str)))
            {
                var keyVals = new Dictionary<string, string>();

                var values = row.Split(fieldSep);

                if (values.Length != header.Length)
                {
                    inconsistentRows = true;
                }

                for (int i = 0; i < header.Length; i++)
                {
                    var value = values[i] ?? string.Empty;

                    if (string.IsNullOrEmpty(value))
                    {
                        continue;
                    }

                    keyVals.Add(header[i], value);
                }

                orders.Add(new ShippingOrder { Fields = keyVals });
            }

            if (inconsistentRows)
            {
                _logger.LogWarning("One or more records in the file did not have the expected number of fields, your label might be missing data");
            }

            return orders;
        }
    }
}
