using Microsoft.Extensions.Logging;
using OlawaleFiledApp.Core.Domain;

namespace OlawaleFiledApp.Core.Data.Repositories.Internal
{
    internal class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(AppDbContext appDbContext, ILogger<PaymentRepository> logger)
        :base(appDbContext, logger)
        {
            
        }
    }
}