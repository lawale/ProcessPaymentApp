using System.Threading.Tasks;

namespace OlawaleFiledApp.Core.Data.Repositories
{
    public interface IUnitOfWork
    {
        /// <summary>
        /// Check For Any Available Transaction
        /// </summary>
        /// <returns></returns>
        bool TransactionExists();
        
        /// <summary>
        /// Starts  a transaction in the DB
        /// </summary>
        /// <remarks>Returns false when a transaction already exists and true when a new transaction is started</remarks>
        /// <returns>Whether or not a new transaction was started</returns>
        Task<bool> StartAsync();
        
        /// <summary>
        /// Commits changes to DB
        /// </summary>
        /// <returns></returns>
        /// <remarks>Disposes transaction after a commit. Start another transaction for another commit</remarks>
        /// <exception cref="System.NullReferenceException">Thrown if transaction does not exist</exception>
        Task CommitAsync();
        
        /// <summary>
        /// Rolls back changes made
        /// </summary>
        /// <returns></returns>
        /// <remarks>Disposes transaction after a rollback. Start another transaction for another rollback</remarks>
        /// <exception cref="System.NullReferenceException">Thrown if transaction does not exist</exception>
        Task RollbackAsync();
        
        IPaymentRepository PaymentRepository { get; }
        
        IPaymentStateRepository PaymentStateRepository { get; }
    }
}