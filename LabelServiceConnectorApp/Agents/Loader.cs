using LabelServiceConnector.Lib.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Windows.Forms;

namespace LabelServiceConnector.Agents
{
    internal class Loader
    {
        private CancellationToken _cancel;

        private ILogger _logger;

        private Action<ToolTipIcon, string> _notificationMethod;

        public Loader(ILogger logger, CancellationToken cancel, Action<ToolTipIcon, string> notificationMethod)
        {
            _cancel = cancel;
            _logger = logger;
            _notificationMethod = notificationMethod;
        }

        public void Start()
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

            int count = 0;

            foreach (var file in files)
            {
                try
                {
                    ShippingOrder order;

                    using (var fileText = new StreamReader(file.FullName, fileEncoding))
                    {
                        order = ParseCSV(fileText.ReadToEnd());
                    }

                    _logger.LogInformation($"'{file.Name}' contains {order.Quantity} parcel(s)");

                    JobQueue.AddJob(new Job(order, file));

                    count++;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Unable to process '{file.Name}', skipping..");
                    _logger.LogDebug($"{ex}: {ex.Message}");

                    _notificationMethod.Invoke(ToolTipIcon.Error, $"Unable to process '{file.Name}', skipping..");

                    Directory.CreateDirectory(dir + "/error/");
                    file.CopyTo(dir + "/error/" + file.Name, overwrite: true);
                }
                finally
                {
                    file.Delete();
                }
            }

            if (count > 0)
            {
                _notificationMethod.Invoke(ToolTipIcon.Info, $"Queued {count} shipping order(s) to be processed");
            }
        }

        private ShippingOrder ParseCSV(string text)
        {
            var fieldSep = Configuration.Config["CsvFieldSeparator"] ?? ";";
            var keyVals = new Dictionary<string, string>();

            var rows = text.Split(Environment.NewLine);
            var header = rows[0].Split(fieldSep);
            var values = rows[1].Split(fieldSep);

            if (values.Length != header.Length)
            {
                _logger.LogWarning("One or more records in the file did not have the expected number of fields, your label might be missing data");
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

            var order = new ShippingOrder()
            {
                Fields = keyVals,
                Quantity = rows.Skip(1)
                               .Where(str => !string.IsNullOrEmpty(str))
                               .Count()
            };

            return order;
        }
    }
}
