using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelServiceConnector.Models
{
    public class Job
    {
        public ShippingOrder ShippingOrder { get; set; }

        public DateTime Touched { get; private set; }

        public JobStatus Status { get; set; }

        public Job(ShippingOrder shippingOrder)
        {
            Touched = DateTime.Now;
            ShippingOrder = shippingOrder;
        }
    }
}
