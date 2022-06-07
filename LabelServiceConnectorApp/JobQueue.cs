using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LabelServiceConnector.Models;

namespace LabelServiceConnector
{
    public static class JobQueue
    {
        public static event EventHandler? JobAdded;

        private static ConcurrentQueue<Job> _queue = new ConcurrentQueue<Job>();

        public static bool JobReady => _queue.Count > 0;

        public static void AddJob(ShippingOrder shippingOrder)
        {
            _queue.Enqueue(new Job(shippingOrder));

            JobAdded?.Invoke(null, EventArgs.Empty);
        }

        public static Job? Next()
        {
            _queue.TryDequeue(out Job? j);

            return j;
        }
    }
}
