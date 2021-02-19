using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OlawaleFiledApp.Core.Data.Exceptions;
using OlawaleFiledApp.Core.Domain;

namespace OlawaleFiledApp.Core.Data.Repositories.Internal
{
    internal abstract class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : BaseEntity
    {
        private readonly ILogger<GenericRepository<TEntity>> logger;
        private readonly DbSet<TEntity> dbTable;

        protected GenericRepository(AppDbContext dbContext, ILogger<GenericRepository<TEntity>> logger)
        {
            this.logger = logger;
            DbContext = dbContext;
            dbTable = dbContext.Set<TEntity>();
        }

        private AppDbContext DbContext { get; }

        public async ValueTask<TEntity?> GetByIdAsync(Guid id)
        {
            var item = await dbTable.FindAsync(id);
            return item?.DeletedAt == null ? item : null!;
        }

        public async ValueTask<TEntity> CreateNewAsync(TEntity item)
        {
            item.Id = Guid.NewGuid();
            await dbTable.AddAsync(item);
            return item;
        }

        public ValueTask UpdateAsync(TEntity item)
        {
            try
            {
                DbContext.Entry(item).State = EntityState.Modified;
                return ValueTask.CompletedTask;
            }
            catch (DbUpdateException e)
            {
                logger.LogError(e, "Unable To Update State Of {0}", typeof(TEntity).Name);
                if(e.InnerException is not null)
                    logger.LogError(e.InnerException, "Update Entity Inner Exception");
                return ValueTask.FromException(new EntityTransactionException("Unable to update the entity",e));
            }
            
        }

        public ValueTask DeleteAsync(TEntity item, bool useSoftDelete)
        {
            try
            {
                if(useSoftDelete)
                    item.DeletedAt = DateTime.UtcNow;;
                
                DbContext.Entry(item).State = useSoftDelete ? EntityState.Modified : EntityState.Deleted;
                
                return ValueTask.CompletedTask;
            }
            catch (DbUpdateException e)
            {
                logger.LogError(e, "Unable To Update State Of {0}", typeof(TEntity).Name);
                if(e.InnerException is not null)
                    logger.LogError(e.InnerException, "Update Entity Inner Exception");
                return ValueTask.FromException(new EntityTransactionException("Unable to delete the entity", e, "Entity delete item"));
            }
        }

        public IQueryable<TEntity> GetQuery() => dbTable;
    }
}