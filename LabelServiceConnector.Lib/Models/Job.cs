using System;
using System.Collections.Generic;
using System.IO;

namespace LabelServiceConnector.Lib.Models
{
    public class Job
    {
        public ShippingOrder ShippingOrder { get; }

        public DateTime Touched { get; }

        public JobStatus Status { get; set; }

        public FileInfo SourceFile { get; }

        public string Id => SourceFile.Name.Split('.')[0];

        public Job(ShippingOrder shippingOrders, FileInfo sourceFile)
        {
            Touched = DateTime.Now;
            ShippingOrder = shippingOrders;
            SourceFile = sourceFile;
        }
    }
}
