using LabelServiceConnector.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
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
                    var so = new ShippingOrder(file);

                    using (var fileText = new StreamReader(file.FullName, fileEncoding))
                    {
                        so.Fields = ParseCSV(fileText.ReadToEnd());
                    }

                    JobQueue.AddJob(so);
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

        private Dictionary<string, string> ParseCSV(string text)
        {
            var fieldSep = Configuration.Config["CsvFieldSeparator"] ?? ";";

            var rows = text.Split(Environment.NewLine);
            var header = rows[0].Split(fieldSep);

            //Flatten remaining rows to permit newlines in the values
            var values = new string(rows
                .Skip(1)
                .SelectMany(s => s + " ")
                .ToArray())
                .Split(fieldSep);

            var kv = new Dictionary<string, string>();

            for (int i = 0; i < header.Length; i++)
            {
                var value = values[i] ?? string.Empty;

                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }

                kv.Add(header[i], value);
            }

            return kv;
        }
    }
}
