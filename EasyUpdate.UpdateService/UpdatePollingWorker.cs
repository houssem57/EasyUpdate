using EasyUpdate.Data;
using EasyUpdate.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace EasyUpdate.UpdateService
{
    public class UpdatePollingWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<UpdatePollingWorker> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(2);

        public UpdatePollingWorker(IServiceProvider serviceProvider, ILogger<UpdatePollingWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("EasyUpdate Scheduler started at {time}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessDueUpdatesAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break; // service is stopping
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while checking for due updates");
                }

                try
                {
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("EasyUpdate Scheduler stopped at {time}", DateTimeOffset.Now);
        }

        private async Task ProcessDueUpdatesAsync(CancellationToken ct)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var executor = scope.ServiceProvider.GetRequiredService<IUpdateExecutor>();

            var dueUpdates = await context.ScheduledUpdates
                .Include(u => u.SoftwareApp)
                .Include(u => u.ScheduledByUser)
                .Where(u => u.Status == UpdateStatus.Pending && u.ScheduledStart <= DateTime.Now)
                .OrderBy(u => u.ScheduledStart)
                .ToListAsync(ct);

            foreach (var update in dueUpdates)
            {
                _logger.LogInformation("Starting update {Id} for {App}", update.Id, update.SoftwareApp.Name);
                await executor.ExecuteUpdateAsync(update, context, ct);
            }
        }
    }
}