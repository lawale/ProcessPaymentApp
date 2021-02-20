using Microsoft.Extensions.Logging;
using OlawaleFiledApp.Core.Domain;

namespace OlawaleFiledApp.Core.Data.Repositories.Internal
{
    internal class PaymentStateRepository : GenericRepository<PaymentState>, IPaymentStateRepository
    {
        public PaymentStateRepository(AppDbContext dbContext, ILogger<PaymentStateRepository> logger) : base(dbContext, logger)
        {
        }
    }
}