using System;
using System.Collections.Generic;

namespace LabelServiceConnector.Lib.Models
{
    public class ShippingOrder
    {
        public ShippingOrder()
        {
            LoadedTime = DateTime.Now;
            Fields = new Dictionary<string, string>();
        }

        public Dictionary<string, string> Fields { get; set; }

        public string? TrackingNumber { get; set; }

        public int Quantity { get; set; }

        public DateTime LoadedTime { get; private set; }
    }
}
