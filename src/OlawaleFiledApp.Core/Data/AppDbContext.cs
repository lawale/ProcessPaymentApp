using System;
using Microsoft.EntityFrameworkCore;

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
            optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
        }
    }
}