using EsiaUserGenerator.Db;
using EsiaUserGenerator.Db.Models;
using EsiaUserGenerator.Db.UoW;

public interface IRequestHistoryRepository : IRepository<UserRequestHistory> { }

public class RequestHistoryRepository : Repository<UserRequestHistory>, IRequestHistoryRepository
{
    public RequestHistoryRepository(ApplicationDbContext context) : base(context) { }
}