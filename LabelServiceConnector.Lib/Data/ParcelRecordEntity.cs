using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace LabelServiceConnector.Lib.Data
{
    [Table("ParcelRecord")]
    public class ParcelRecordEntity
    {
        public int Id { get; set; }

        public string TrackingNumber { get; set; } = "unknown";

        public DateTime ShipmentDate { get; set; }

        public DateTime? ArchivedOn { get; set; }

        public DeliveryStatus Status { get; set; }

        public string? ArchivedFilePath { get; set; }

        public enum DeliveryStatus
        {
            Pending = 0,
            Delivered,
            NotDelivered
        }
    }
}
