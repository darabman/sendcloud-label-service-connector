using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabelServiceConnector.Models
{
    public enum JobStatus
    {
        Error = -1,
        None = 0,
        Queued,
        Fetching,
        Printing,
        Saving,
        Complete
    }
}
