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
        private static ConcurrentQueue<Job> _queue = new ConcurrentQueue<Job>();

        public static bool JobReady => _queue.Count > 0;

        public static void AddJob(ShippingOrder shippingOrder)
        {
            _queue.Enqueue(new Job(shippingOrder));
        }

        public static Job? Next()
        {
            Job j;

            while (_queue.TryDequeue(out j));

            return j;
        }
    }
}
