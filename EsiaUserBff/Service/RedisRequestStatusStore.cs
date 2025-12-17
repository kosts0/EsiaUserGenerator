using EsiaUserGenerator.Service.Interface;
using StackExchange.Redis;

namespace EsiaUserGenerator.Service;
[Obsolete]
public class RedisRequestStatusStore : IRequestStatusStore
{
    private readonly IDatabase _db;

    public RedisRequestStatusStore()
    {
        return;
        //_db = multiplexer.GetDatabase();
    }

    public async Task SetStatusAsync(string requestId, string status)
    {
        return;
        await _db.StringSetAsync($"req:{requestId}", status, TimeSpan.FromMinutes(10));
    }

    public async Task<string?> GetStatusAsync(string requestId)
    {
        return string.Empty;
        return await _db.StringGetAsync($"req:{requestId}");
    }
}