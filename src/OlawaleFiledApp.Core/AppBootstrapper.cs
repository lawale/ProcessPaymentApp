using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OlawaleFiledApp.Core.Constants;
using OlawaleFiledApp.Core.Data;
using OlawaleFiledApp.Core.Data.Repositories;
using OlawaleFiledApp.Core.Services.Payments.Gateways;
using OlawaleFiledApp.Core.Services.Payments.Gateways.Implementations;

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
            services.AddHttpClient();
            
            services.AddHttpClient<IPaymentGateway, PremiumPaymentGateway>() //custom http client for premium retry policy
                .AddPolicyHandler(GatewayPolicy.PremiumRetryPolicy());
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