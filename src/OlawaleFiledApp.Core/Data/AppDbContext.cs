using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OlawaleFiledApp.Core.Domain;
using OlawaleFiledApp.Core.Models.Constants;

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

        public DbSet<Payment> Payments { get; set; } = null!;

        public DbSet<PaymentState> PaymentStates { get; set; } = null!;
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (dco is not null!)
            {
                base.OnConfiguring(optionsBuilder);
                return;
            }
            var connection = new SqliteConnection($"Data Source={StringConstants.ConnectionString};Cache=Shared;Mode=Memory");
            connection.Open();
            optionsBuilder.UseSqlite(connection, b => b.MigrationsAssembly("OlawaleFiledApp.Api"));
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Payment>()
                .HasMany<PaymentState>()
                .WithOne(x => x.Payment)
                .HasForeignKey(x => x.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public async Task<bool> TrySaveChangesAsync(ILogger logger)
        {
            try
            {
                var res = await SaveChangesAsync();
                logger.LogInformation("{0} Operations Completed In the DB", res);
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