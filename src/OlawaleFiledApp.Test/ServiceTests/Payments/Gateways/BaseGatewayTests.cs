using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using OlawaleFiledApp.Core;
using OlawaleFiledApp.Core.Data;
using OlawaleFiledApp.Core.Models.Payloads;
using OlawaleFiledApp.Core.Services.Payments.Gateways;
using OlawaleFiledApp.Core.Services.Payments.Gateways.Factory;

namespace OlawaleFiledApp.Test.ServiceTests.Payments.Gateways
{
    public class BaseGatewayTests
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

        [Test]
        public async Task Test_ChargeCardAsync_ReturnSuccess()
        {
            serviceCollection.AddTransient(xy =>
            {
                var moqObj = new Mock<IHttpClientFactory>();
                var message = new Mock<HttpMessageHandler>();
                message.Protected()
                    .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("{\"status\" : true}", Encoding.UTF8, "application/json")
                    });
                var client = new HttpClient(message.Object);
                moqObj.Setup(x => x.CreateClient(It.IsAny<string>())).Returns<string>(x => client);
                return moqObj.Object;
            });

            var container = serviceCollection.BuildServiceProvider();

            var gatewayFactory = container.GetService<IPaymentGatewayFactory>();
            Assert.NotNull(gatewayFactory);
            
            var gateway = gatewayFactory.ResolveGateway(PaymentType.Cheap);
            Assert.NotNull(gateway);

            var result = await gateway.ChargeCardAsync(new PaymentPayload());
            
            Assert.NotNull(result);
            Assert.IsTrue(result);
        }
        
        [Test]
        public async Task Test_ChargeCardAsync_ReturnFalse()
        {
            serviceCollection.AddTransient(xy =>
            {
                var moqObj = new Mock<IHttpClientFactory>();
                var message = new Mock<HttpMessageHandler>();
                message.Protected()
                    .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ReturnsAsync(new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("{\"status\" : \"false\"}", Encoding.UTF8, "application/json")
                    });
                var client = new HttpClient(message.Object);
                moqObj.Setup(x => x.CreateClient(It.IsAny<string>())).Returns<string>(x => client);
                return moqObj.Object;
            });

            var container = serviceCollection.BuildServiceProvider();

            var gatewayFactory = container.GetService<IPaymentGatewayFactory>();
            Assert.NotNull(gatewayFactory);
            
            var gateway = gatewayFactory.ResolveGateway(PaymentType.Cheap);
            Assert.NotNull(gateway);

            var result = await gateway.ChargeCardAsync(new PaymentPayload());
            
            Assert.NotNull(result);
            Assert.IsFalse(result);
        }
        
        [Test]
        public async Task Test_ChargeCardAsync_ReturnServiceError()
        {
            serviceCollection.AddTransient(xy =>
            {
                var moqObj = new Mock<IHttpClientFactory>();
                var message = new Mock<HttpMessageHandler>();
                message.Protected()
                    .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                        ItExpr.IsAny<CancellationToken>())
                    .ThrowsAsync(new HttpRequestException());
                var client = new HttpClient(message.Object);
                moqObj.Setup(x => x.CreateClient(It.IsAny<string>())).Returns<string>(x => client);
                return moqObj.Object;
            });

            var container = serviceCollection.BuildServiceProvider();

            var gatewayFactory = container.GetService<IPaymentGatewayFactory>();
            Assert.NotNull(gatewayFactory);
            
            var gateway = gatewayFactory.ResolveGateway(PaymentType.Premium);
            Assert.NotNull(gateway);

            var result = await gateway.ChargeCardAsync(new PaymentPayload());
            
            Assert.IsNull(result);
        }
    }
}