using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OlawaleFiledApp.Core.Constants;
using OlawaleFiledApp.Core.Data;
using OlawaleFiledApp.Core.Data.Repositories;

namespace OlawaleFiledApp.Core
{
    public static class AppBootstrapper
    {
        /// <summary>
        /// Registers All Services In the core project into container
        /// </summary>
        /// <param name="services"></param>
        public static void InitCoreServicesAndRepositories(this IServiceCollection services)
        {
            AutoInjectLayers(services);
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddDbContext<AppDbContext>(x => x.UseInMemoryDatabase(StringConstants.ConnectionString));
        }
        
        private static void AutoInjectLayers(IServiceCollection serviceCollection)
        {
            serviceCollection.Scan(scan => scan.FromCallingAssembly().AddClasses(classes => classes
                    .Where(type => type.Name.EndsWith("Repository") || type.Name.EndsWith("Service")),false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        }
    }
}