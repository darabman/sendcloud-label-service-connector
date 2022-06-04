using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LabelServiceConnector
{
    public static class SOLoader
    {
        public static event EventHandler<Models.ShippingOrder>? ShippingOrderAvailable;

        public static void ScanDirectory(string path, ILogger logger)
        {
            var dir = new DirectoryInfo(path);
            var shippingOrders = new List<Models.ShippingOrder>();
            var files = dir.GetFiles("*.csv").OrderBy(f => f.CreationTime);

            logger.LogInformation($"Found {files.Count()} CSV files in '{path}'");

            foreach (var file in files)
            {
                try
                {
                    var so = new Models.ShippingOrder(file);

                    using (var fileText = file.OpenText())
                    {
                        so.Fields = ParseCSV(fileText.ReadToEnd());
                    }

                    ShippingOrderAvailable?.Invoke(null, so);
                }
                catch
                {
                    logger.LogWarning($"Unable to process '{file.Name}', skipping..");
                }
            }
        }

        private static Dictionary<string, string> ParseCSV(string text)
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
