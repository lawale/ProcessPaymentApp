using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OlawaleFiledApp.Core.Data;
using OlawaleFiledApp.Core.Data.Repositories;
using OlawaleFiledApp.Core.Models.Constants;
using OlawaleFiledApp.Core.Services.Payments.Gateways;
using OlawaleFiledApp.Core.Services.Payments.Gateways.Factory;
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
            AutoInjectLayers(services);
            
            services.AddHttpClient();
            services.AddHttpClient(PaymentType.Premium.ToString()) //named http client for premium retry policy
                .AddPolicyHandler(GatewayPolicy.PremiumRetryPolicy());
            
            services.AddTransient<IPaymentGatewayFactory, PaymentGatewayFactory>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
        
        private static void AutoInjectLayers(IServiceCollection serviceCollection)
        {
            serviceCollection.Scan(scan => scan.FromCallingAssembly().AddClasses(classes => classes
                    .Where(type => type.Name.EndsWith("Repository") || type.Name.EndsWith("Service") || type.Name.EndsWith("Gateway")),false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        }
    }
}