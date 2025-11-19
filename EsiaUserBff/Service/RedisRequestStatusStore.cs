using EsiaUserGenerator.Service.Interface;
using StackExchange.Redis;

namespace EsiaUserGenerator.Service;

public class RedisRequestStatusStore : IRequestStatusStore
{
    private readonly IDatabase _db;

    public RedisRequestStatusStore(IConnectionMultiplexer multiplexer)
    {
        _db = multiplexer.GetDatabase();
    }

    public async Task SetStatusAsync(string requestId, string status)
    {
        await _db.StringSetAsync($"req:{requestId}", status, TimeSpan.FromMinutes(10));
    }

    public async Task<string?> GetStatusAsync(string requestId)
    {
        return await _db.StringGetAsync($"req:{requestId}");
    }
}