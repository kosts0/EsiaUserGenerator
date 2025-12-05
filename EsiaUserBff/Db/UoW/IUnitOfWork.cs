namespace EsiaUserGenerator.Db.UoW;

public interface IUnitOfWork : IAsyncDisposable
{
    IEsiaUserRepository Users { get; }
    IRequestHistoryRepository RequestHistory { get; }

    Task<int> CompleteAsync();
}
