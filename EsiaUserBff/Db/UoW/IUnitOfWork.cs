namespace EsiaUserGenerator.Db.UoW;

public interface IUnitOfWork : IAsyncDisposable
{
    IEsiaUserRepository Users { get; }
    //ICreatedHistoryRepository CreatedHistory { get; }
    IRequestHistoryRepository RequestHistory { get; }

    Task<int> CompleteAsync();
}
