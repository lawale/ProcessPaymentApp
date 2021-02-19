using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace OlawaleFiledApp.Core.Data.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext dbContext;
        private readonly ILogger<UnitOfWork> logger;
        private IDbContextTransaction transaction = null!;

        public UnitOfWork(AppDbContext dbContext, ILogger<UnitOfWork> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }
        
        public async Task<bool> StartAsync()
        {
            if (transaction is not null!)
            {
                logger.LogWarning("Transaction {0} already started. Cannot Start new transaction", transaction.TransactionId);
                return false;
            }
            transaction = await dbContext.Database.BeginTransactionAsync();
            logger.LogInformation("A unit of work {0} has been initialized", transaction.TransactionId);

            return true;
        }

        public async Task CommitAsync()
        {
            ValidateTransaction();
            if (!await dbContext.TrySaveChangesAsync(logger))
            {
                logger.LogWarning("Unit of work has failed to commit");
                return;
            }
            
            await transaction.CommitAsync();

            await DisposeTransactionAsync();
        }

        public async Task RollbackAsync()
        {
            ValidateTransaction();
            
            await transaction.RollbackAsync();

            await DisposeTransactionAsync();
        }

        private void ValidateTransaction()
        {
            if (transaction is not null!) return;
            
            logger.LogCritical("No transaction was found for commit made");
            throw new NullReferenceException("No Transaction Has Been Started");
        }
        
        private async Task DisposeTransactionAsync()
        {
            await transaction.DisposeAsync();

            transaction = null!;
        }
    }
}