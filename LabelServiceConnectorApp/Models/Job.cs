using System;
using System.Collections.Generic;
using System.IO;

namespace LabelServiceConnector.Models
{
    public class Job
    {
        public List<ShippingOrder> ShippingOrders { get; }

        public DateTime Touched { get; }

        public JobStatus Status { get; set; }

        public FileInfo SourceFile { get; }

        public string Id => SourceFile.Name.Split('.')[0];

        public Job(List<ShippingOrder> shippingOrders, FileInfo sourceFile)
        {
            Touched = DateTime.Now;
            ShippingOrders = shippingOrders;
            SourceFile = sourceFile;
        }
    }
}
