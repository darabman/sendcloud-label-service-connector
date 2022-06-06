using LabelServiceConnector.Models;
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
                int delayMs = 1000;

                while (!_cancel.IsCancellationRequested)
                {
                    ScanDirectory("./");
                    Thread.Sleep(delayMs);
                }

            }).Start();
        }

        public void ScanDirectory(string path)
        {
            var dir = new DirectoryInfo(path);
            var shippingOrders = new List<ShippingOrder>();
            var files = dir.GetFiles("*.csv").OrderBy(f => f.CreationTime);

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
                catch
                {
                    _logger.LogWarning($"Unable to process '{file.Name}', skipping..");
                }
            }
        }

        private Dictionary<string, string> ParseCSV(string text)
        {
            var rows = text.Split('\n');

            var header = rows[0].Split(';');
            var values = rows[1].Split(';');

            var kv = new Dictionary<string, string>();

            for (int i = 0; i < header.Length; i++)
            {
                kv.Add(header[i], values[i] ?? "");
            }

            return kv;
        }
    }
}
