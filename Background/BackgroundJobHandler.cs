using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PochinkiBot.Background
{
    public class BackgroundJobHandler
    {
        private class WorkItem
        {
            public Guid Id { get; } = Guid.NewGuid();
            public Func<Task> Work { get; set; }
            public DateTime Time { get; set; }

            public override bool Equals(object obj)
            {
                return Id.Equals(obj);
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }
        
        private Thread _jobThread;
        private bool _isCancellationRequested;
        private readonly ConcurrentDictionary<Guid, WorkItem> _jobQueue = new ConcurrentDictionary<Guid, WorkItem>();

        public void Run()
        {
            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => Stop();
            _jobThread = new Thread(JobProcessing);
            _jobThread.Start();
        }

        private void JobProcessing()
        {
            while (!_isCancellationRequested)
            {
                Thread.Sleep(10);
                if (_isCancellationRequested)
                    return;

                var expired = new HashSet<WorkItem>();
                var now = DateTime.UtcNow;
                foreach (var (_, job) in _jobQueue)
                {
                    if (job.Time > now) 
                        continue;
                    expired.Add(job);
                    Task.Run(job.Work);
                }

                foreach (var item in expired)
                {
                    _jobQueue.TryRemove(item.Id, out _);
                }
            }
        }

        public void Stop()
        {
            _isCancellationRequested = true;
            _jobThread.Join();
        }

        public void Enqueue(Func<Task> workItem, TimeSpan? delay = null)
        {
            var item = new WorkItem
            {
                Time = DateTime.UtcNow.Add(delay ?? TimeSpan.FromSeconds(10)),
                Work = workItem
            };

            _jobQueue.TryAdd(item.Id, item);
        }
    }
}