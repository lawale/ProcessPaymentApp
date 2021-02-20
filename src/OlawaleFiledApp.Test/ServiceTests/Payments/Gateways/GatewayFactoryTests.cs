using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using OlawaleFiledApp.Core;
using OlawaleFiledApp.Core.Data;
using OlawaleFiledApp.Core.Services.Payments.Gateways;
using OlawaleFiledApp.Core.Services.Payments.Gateways.Exceptions;
using OlawaleFiledApp.Core.Services.Payments.Gateways.Factory;
using OlawaleFiledApp.Core.Services.Payments.Gateways.Implementations;

namespace OlawaleFiledApp.Test.ServiceTests.Payments.Gateways
{
    public class GatewayFactoryTests
    {
        private ServiceCollection serviceCollection;

        [SetUp]
        public void Setup()
        {
            serviceCollection = new ServiceCollection();
            
            serviceCollection.AddLogging();
            serviceCollection.InitCoreServicesAndRepositories();
            serviceCollection.AddDbContext<AppDbContext>(x =>
            {
                var connection = new SqliteConnection($"Data Source={Guid.NewGuid()};Mode=Memory");
                connection.Open();
                x.UseSqlite(connection);
            });
            
            serviceCollection.AddScoped(xy =>
            {
                var moqObj = new Mock<IConfiguration>();
                moqObj.Setup(x => x["GatewayUrls:Cheap"]).Returns("http://test-gateway.com/cheap");
                moqObj.Setup(x => x["GatewayUrls:Expensive"]).Returns("http://test-gateway.com/expensive");
                moqObj.Setup(x => x["GatewayUrls:Premium"]).Returns("http://test-gateway.com/premium");
                return moqObj.Object;
            });
        }

        public static IEnumerable<PaymentType> PaymentTypes
        {
            get
            {
                yield return PaymentType.Cheap;
                yield return PaymentType.Expensive;
                yield return PaymentType.Premium;
            }
        }

        [Test]
        [TestCaseSource(nameof(PaymentTypes))]
        public void Test_ResolveGateway_AllGatewaysAvailable(PaymentType paymentType)
        {
            var container = serviceCollection.BuildServiceProvider();
            
            var factory = container.GetService<IPaymentGatewayFactory>();
            Assert.NotNull(factory);

            var gateway = factory.ResolveGateway(paymentType);
            Assert.Multiple(() =>
            {
                Assert.NotNull(gateway);
                Assert.AreEqual(paymentType, gateway.Type);
            });
        }
        
        [Test]
        [TestCaseSource(nameof(PaymentTypes))]
        public void Test_ResolveGateway_CheapGatewayUnavailable(PaymentType paymentType)
        {
            var paymentGateway = serviceCollection.FirstOrDefault(x => x.ImplementationType == typeof(CheapPaymentGateway));
            serviceCollection.Remove(paymentGateway);
            
            var container = serviceCollection.BuildServiceProvider();
            
            var factory = container.GetService<IPaymentGatewayFactory>();
            Assert.NotNull(factory);

            var gateway = factory.ResolveGateway(paymentType);

            if (paymentType == PaymentType.Cheap)
            {
                Assert.Null(gateway);
                return;
            }
            
            Assert.Multiple(() =>
            {
                Assert.NotNull(gateway);
                Assert.AreEqual(paymentType, gateway.Type);
            });
        }
        
        [Test]
        [TestCaseSource(nameof(PaymentTypes))]
        public void Test_ResolveGateway_AllGatewaysUnavailable(PaymentType paymentType)
        {
            serviceCollection.RemoveAll(typeof(IPaymentGateway));
            var container = serviceCollection.BuildServiceProvider();
            
            var factory = container.GetService<IPaymentGatewayFactory>();
            Assert.NotNull(factory);

            Assert.Catch<PaymentGatewayException>(() =>
            {
                factory.ResolveGateway(paymentType);
            });
        }
    }
}