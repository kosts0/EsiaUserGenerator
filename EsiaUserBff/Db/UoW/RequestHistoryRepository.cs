using EsiaUserGenerator.Db;
using EsiaUserGenerator.Db.Models;
using EsiaUserGenerator.Db.UoW;

public interface IRequestHistoryRepository : IRepository<RequestHistory> { }

public class RequestHistoryRepository : Repository<RequestHistory>, IRequestHistoryRepository
{
    public RequestHistoryRepository(ApplicationDbContext context) : base(context) { }
}