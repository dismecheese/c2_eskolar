using c2_eskolar.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace c2_eskolar.BackgroundServices
{
    /// <summary>
    /// Background service that runs monthly aggregation at the start of each month
    /// </summary>
    public class MonthlyAggregationBackgroundService : BackgroundService
    {
        private readonly ILogger<MonthlyAggregationBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private Timer? _timer;

        public MonthlyAggregationBackgroundService(
            ILogger<MonthlyAggregationBackgroundService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Monthly Aggregation Background Service started");

            // Calculate time until next month
            var now = DateTime.Now;
            var nextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);
            var timeUntilNextMonth = nextMonth - now;

            // Add 1 hour buffer to ensure month has fully passed
            timeUntilNextMonth = timeUntilNextMonth.Add(TimeSpan.FromHours(1));

            _logger.LogInformation($"Next aggregation scheduled for {nextMonth.AddHours(1):yyyy-MM-dd HH:mm:ss}");

            // Set up timer to run at the start of each month
            _timer = new Timer(
                DoWork,
                null,
                timeUntilNextMonth,
                TimeSpan.FromDays(30)); // Approximate monthly interval

            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            try
            {
                _logger.LogInformation("Starting monthly aggregation job");

                using (var scope = _serviceProvider.CreateScope())
                {
                    var statsService = scope.ServiceProvider.GetRequiredService<MonthlyStatisticsService>();
                    await statsService.AggregatePreviousMonthAsync();
                }

                _logger.LogInformation("Monthly aggregation job completed successfully");

                // Recalculate next run time to be accurate
                var now = DateTime.Now;
                var nextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddHours(1);
                var timeUntilNextMonth = nextMonth - now;

                _timer?.Change(timeUntilNextMonth, TimeSpan.FromDays(30));
                _logger.LogInformation($"Next aggregation scheduled for {nextMonth:yyyy-MM-dd HH:mm:ss}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during monthly aggregation job");
            }
        }

        public override void Dispose()
        {
            _timer?.Dispose();
            base.Dispose();
        }
    }
}
