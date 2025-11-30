namespace EsiaUserGenerator.Db.UoW;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public IEsiaUserRepository Users { get; }
    public ICreatedHistoryRepository CreatedHistory { get; }
    public IRequestHistoryRepository RequestHistory { get; }

    public UnitOfWork(
        ApplicationDbContext context,
        IEsiaUserRepository users,
        ICreatedHistoryRepository createdHistory,
        IRequestHistoryRepository requestHistory)
    {
        _context = context;

        Users = users;
        CreatedHistory = createdHistory;
        RequestHistory = requestHistory;
    }

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}
