using EsiaUserGenerator.Service.Interface;

namespace EsiaUserGenerator.Service;

public class Worker : BackgroundService
{
    private readonly IBackgroundTaskQueue _queue;
    private readonly ILogger<Worker> _logger;

    public Worker(IBackgroundTaskQueue queue, ILogger<Worker> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await _queue.DequeueAsync(stoppingToken);
            _ = Task.Run(async () =>
            {
                try
                {
                    _logger.LogDebug("Starting task executing");
                    await workItem(stoppingToken);
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Background task failed");
                }
            }, stoppingToken);
        }
    }
}