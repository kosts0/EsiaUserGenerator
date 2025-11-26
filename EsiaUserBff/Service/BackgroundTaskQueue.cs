using System.Threading.Channels;
using EsiaUserGenerator.Service.Interface;

namespace EsiaUserGenerator.Service;

public sealed class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<IServiceProvider, CancellationToken, Task>> _queue;

    public BackgroundTaskQueue()
    {
        _queue = Channel.CreateUnbounded<Func<IServiceProvider, CancellationToken, Task>>();
    }

    public ValueTask QueueAsync(Func<IServiceProvider, CancellationToken, Task> workItem)
        => _queue.Writer.WriteAsync(workItem);

    public async ValueTask<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken ct)
        => await _queue.Reader.ReadAsync(ct);
}
