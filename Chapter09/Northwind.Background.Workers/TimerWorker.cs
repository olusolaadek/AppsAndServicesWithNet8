using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Northwind.Background.Workers
{
    public class TimerWorker : IHostedService, IAsyncDisposable
    {
        private readonly ILogger<TimerWorker> _logger;


        private int _executionCount = 0;
        private Timer? _timer;
        private int _seconds = 5;

        public TimerWorker(ILogger<TimerWorker> logger)
        {
            _logger = logger;
        }

        private void DoWork(object? state)
        {
            int counter = Interlocked.Increment(ref _executionCount);
            _logger.LogInformation("{0} is working. Count: {1}", nameof(TimerWorker), counter);
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("{0} is running.", nameof(TimerWorker));

            _timer = new Timer(DoWork, null,TimeSpan.Zero, TimeSpan.FromSeconds(_seconds));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("{0} is stopping.", nameof(TimerWorker));
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            if (_timer is IAsyncDisposable asyncTimer)
            {
                await asyncTimer.DisposeAsync();
            }
            _timer= null;
            // _timer?.Dispose();
            //await Task.CompletedTask;
        }
    }
}
