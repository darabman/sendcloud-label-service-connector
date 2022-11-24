using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelServiceConnector.Models
{
    public class ArchivedParcelRecord
    {
        [Key]
        public string? TrackingNumber { get; set; }

        public DateTime ShipmentDate { get; set; }

        public FileInfo? ArchivedFilePath { get; set; }
    }
   
}
