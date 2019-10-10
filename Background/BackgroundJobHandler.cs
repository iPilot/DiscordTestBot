using System;
using System.Threading;
using Hangfire;

namespace PochinkiBot.Background
{
    public class BackgroundJobHandler
    {
        private readonly Thread _jobThread;
        private readonly CancellationTokenSource _tokenSource;

        public BackgroundJobHandler()
        {
            _jobThread = new Thread(JobProcessing);
            _tokenSource = new CancellationTokenSource();
        }

        public void Run()
        {
            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => Stop();
            _jobThread.Start();
        }

        private async void JobProcessing()
        {
            var options = new BackgroundJobServerOptions
            {
                WorkerCount = 1,
                ServerName = "Pochinki-Bot",
                SchedulePollingInterval = TimeSpan.FromSeconds(10)
            };

            using (var jobServer = new BackgroundJobServer(options))
            {
                await jobServer.WaitForShutdownAsync(_tokenSource.Token);
            }
        }

        public void Stop()
        {
            _tokenSource.Cancel();
            _jobThread.Join();
        }
    }
}