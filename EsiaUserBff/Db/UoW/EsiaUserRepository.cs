using EsiaUserGenerator.Db;
using EsiaUserGenerator.Db.Models;
using EsiaUserGenerator.Db.UoW;
using Microsoft.EntityFrameworkCore;

public interface IEsiaUserRepository : IRepository<EsiaUser>
{
    Task<EsiaUser?> GetByOidAsync(string oid);
}

public class EsiaUserRepository : Repository<EsiaUser>, IEsiaUserRepository
{
    public EsiaUserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<EsiaUser?> GetByOidAsync(string oid) =>
        await _dbSet.FirstOrDefaultAsync(x => x.Oid == oid);
}