using EsiaUserGenerator.Service.Interface;

namespace EsiaUserGenerator.Service;

public sealed class WorkerBackgroundService : BackgroundService
{
    private readonly IServiceProvider _provider;
    private readonly IBackgroundTaskQueue _queue;

    public WorkerBackgroundService(IServiceProvider provider, IBackgroundTaskQueue queue)
    {
        _provider = provider;
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await _queue.DequeueAsync(stoppingToken);

            _ = Task.Run(async () =>
            {
                using var scope = _provider.CreateScope();
                try
                {
                    await workItem(scope.ServiceProvider, stoppingToken);
                }
                catch (System.Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<WorkerBackgroundService>>();
                    logger.LogError(ex, "Background task failed");
                }
            });
        }
    }
}
