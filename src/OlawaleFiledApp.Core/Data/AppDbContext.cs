using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OlawaleFiledApp.Core.Constants;

namespace OlawaleFiledApp.Core.Data
{
    public class AppDbContext : DbContext
    {
        private readonly DbContextOptions dco;

        public AppDbContext()
        {
            //dco value is set in OnConfiguring Method
            dco = null!; //set to null forgiving to suppress warnings
        }

        public AppDbContext(DbContextOptions<AppDbContext> dco) : base(dco)
        {
            this.dco = dco;
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (dco is not null!)
            {
                base.OnConfiguring(optionsBuilder);
                return;
            }
            optionsBuilder.UseInMemoryDatabase(StringConstants.ConnectionString);
        }
        
        public async Task<bool> TrySaveChangesAsync(ILogger logger)
        {
            try
            {
                await SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException e)
            {
                logger.LogCritical(e, "Could Not Save Changes To DB");
                if(e.InnerException is not null)
                    logger.LogCritical(e.InnerException, "Inner Exception For DBUpdate Exception");
                return false;
            }
        }
    }
}