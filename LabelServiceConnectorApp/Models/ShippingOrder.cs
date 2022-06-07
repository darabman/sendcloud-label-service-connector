using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelServiceConnector.Models
{
    public class ShippingOrder
    {
        public ShippingOrder(FileInfo fileInformation)
        {
            FileInformation = fileInformation;
            LoadedTime = DateTime.Now;
        }

        public string Id => FileInformation.Name.Split('.')[0];

        public Dictionary<string, string>? Fields { get; set; }

        public string? TrackingNumber { get; set; }

        public FileInfo FileInformation { get; }

        public DateTime LoadedTime { get; private set; }
    }
}
