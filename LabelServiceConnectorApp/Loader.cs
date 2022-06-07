﻿using LabelServiceConnector.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


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
                int delayMs = int.Parse(Configuration.App["CsvScanRateMs"] ?? "1000");

                while (!_cancel.IsCancellationRequested)
                {
                    ScanDirectory(Configuration.App["CsvInputDir"] ?? "./");
                    Thread.Sleep(delayMs);
                }

            }).Start();
        }

        public void ScanDirectory(string path)
        {
            var dir = new DirectoryInfo(path);
            var shippingOrders = new List<ShippingOrder>();
            var files = dir.GetFiles("*.csv").OrderBy(f => f.CreationTime);

            if (files.Any())
                _logger.LogInformation($"Found {files.Count()} CSV files in '{path}'");

            foreach (var file in files)
            {
                try
                {
                    var so = new ShippingOrder(file);

                    using (var fileText = file.OpenText())
                    {
                        so.Fields = ParseCSV(fileText.ReadToEnd());
                    }

                    file.Delete();

                    JobQueue.AddJob(so);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Unable to process '{file.Name}', skipping..");
                    _logger.LogDebug($"{ex}: {ex.Message}");
                }
            }
        }

        private Dictionary<string, string> ParseCSV(string text)
        {
            var rowSep = Configuration.App["CsvRowSeparator"] ?? "\n";
            var fieldSep = Configuration.App["CsvFieldSeparator"] ?? ";";

            var rows = text.Split(rowSep);
            
            var header = rows[0].Split(fieldSep);
            var values = rows[1].Split(fieldSep);

            var kv = new Dictionary<string, string>();

            for (int i = 0; i < header.Length; i++)
            {
                kv.Add(header[i], values[i] ?? "");
            }

            return kv;
        }
    }
}
