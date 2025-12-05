using EsiaUserGenerator.Db;
using EsiaUserGenerator.Db.Models;
using EsiaUserGenerator.Db.UoW;
using Microsoft.EntityFrameworkCore;

public interface IEsiaUserRepository : IRepository<EsiaUser>
{
    Task<EsiaUser?> GetByOidAsync(string oid);
    Task<IEnumerable<EsiaUser>> GetAllLazyAsync();
}

public class EsiaUserRepository : Repository<EsiaUser>, IEsiaUserRepository
{
    public EsiaUserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<EsiaUser?> GetByOidAsync(string oid) =>
        await _dbSet.Include(u => u.RequestData).FirstOrDefaultAsync(x => x.Oid == oid);

    public async Task<IEnumerable<EsiaUser>> GetAllLazyAsync() => await _dbSet.Include(u => u.RequestData).ToListAsync();
}