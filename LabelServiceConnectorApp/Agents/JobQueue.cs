﻿using System;
using System.Collections.Concurrent;
using LabelServiceConnector.Lib.Models;

namespace LabelServiceConnector.Agents
{
    public static class JobQueue
    {
        public static event EventHandler? JobAdded;

        private static ConcurrentQueue<Job> _queue = new ConcurrentQueue<Job>();

        public static bool JobReady => _queue.Count > 0;

        public static void AddJob(Job job)
        {
            _queue.Enqueue(job);

            JobAdded?.Invoke(null, EventArgs.Empty);
        }

        public static Job? Next()
        {
            _queue.TryDequeue(out Job? j);

            return j;
        }
    }
}
