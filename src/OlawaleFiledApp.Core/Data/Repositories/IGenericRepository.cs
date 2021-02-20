using System;
using System.Linq;
using System.Threading.Tasks;
using OlawaleFiledApp.Core.Domain;

namespace OlawaleFiledApp.Core.Data.Repositories
{
    public interface IGenericRepository<TEntity> where TEntity : BaseEntity
    {
        ValueTask<TEntity?> GetByIdAsync(Guid id);
        ValueTask<TEntity> CreateNewAsync(TEntity item);
        ValueTask UpdateAsync(TEntity item);
        ValueTask DeleteAsync(TEntity item, bool useSoftDelete = false);
        IQueryable<TEntity> GetQuery();
    }
}