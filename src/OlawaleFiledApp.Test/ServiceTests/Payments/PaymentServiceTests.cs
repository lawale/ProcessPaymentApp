using System;
using System.Collections.Generic;
using System.Linq;
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
using OlawaleFiledApp.Core.Domain;
using OlawaleFiledApp.Core.Models.Payloads;
using OlawaleFiledApp.Core.Models.Resources;
using OlawaleFiledApp.Core.Services.Payments;
using OlawaleFiledApp.Core.Services.Payments.Gateways.Implementations;

namespace OlawaleFiledApp.Test.ServiceTests.Payments
{
    public class PaymentServiceTests
    {
        private ServiceCollection serviceCollection;

        public static IEnumerable<decimal> Amounts
        {
            get
            {
                yield return 18M;
                yield return 300M;
                yield return 1500M;
            }
        }

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
        [TestCaseSource(nameof(Amounts))]
        public async Task Test_ProcessPayment_PaymentProcessed(decimal amount)
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

            var paymentService = container.GetService<IPaymentService>();
            
            Assert.NotNull(paymentService);

            var result = await paymentService.ProcessAsync(new PaymentPayload
            {
                Amount = amount, CardHolder = "Olawale Lawal", CreditCardNumber = "2648-3783-4934-8938"
            });
            
            Assert.Multiple(() =>
            {
                Assert.NotNull(result);
                Assert.AreEqual(ResponseType.Created, result.ResponseType);
                Assert.NotNull(result.Data);
                Assert.AreEqual(PaymentResult.Processed.ToString(), result.Data.PaymentState);
            });
        }
        
        [Test]
        [TestCaseSource(nameof(Amounts))]
        public async Task Test_ProcessPayment_PaymentFailed(decimal amount)
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
                        Content = new StringContent("{\"status\" : false}", Encoding.UTF8, "application/json")
                    });
                var client = new HttpClient(message.Object);
                moqObj.Setup(x => x.CreateClient(It.IsAny<string>())).Returns<string>(x => client);
                return moqObj.Object;
            });

            var container = serviceCollection.BuildServiceProvider();

            var paymentService = container.GetService<IPaymentService>();
            
            Assert.NotNull(paymentService);

            var result = await paymentService.ProcessAsync(new PaymentPayload
            {
                Amount = amount, CardHolder = "Olawale Lawal", CreditCardNumber = "2648-3783-4934-8938"
            });
            
            Assert.Multiple(() =>
            {
                Assert.NotNull(result);
                Assert.AreEqual(ResponseType.ServiceError, result.ResponseType);
                Assert.NotNull(result.Data);
                Assert.AreEqual(PaymentResult.Failed.ToString(), result.Data.PaymentState);
            });
        }
        
        [Test]
        [TestCaseSource(nameof(Amounts))]
        public async Task Test_ProcessPayment_PaymentPending(decimal amount)
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

            var paymentService = container.GetService<IPaymentService>();
            
            Assert.NotNull(paymentService);

            var result = await paymentService.ProcessAsync(new PaymentPayload
            {
                Amount = amount, CardHolder = "Olawale Lawal", CreditCardNumber = "2648-3783-4934-8938"
            });
            
            Assert.Multiple(() =>
            {
                Assert.NotNull(result);
                Assert.AreEqual(ResponseType.ServiceError, result.ResponseType);
                Assert.NotNull(result.Data);
                Assert.AreEqual(PaymentResult.Pending.ToString(), result.Data.PaymentState);
            });
        }
        
        [Test]
        public async Task Test_ProcessPayment_ExpensivePaymentProcessedAsCheap()
        {
            var paymentGateway = serviceCollection.FirstOrDefault(x => x.ImplementationType == typeof(ExpensivePaymentGateway));
            serviceCollection.Remove(paymentGateway);
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

            var paymentService = container.GetService<IPaymentService>();

            var result = await paymentService.ProcessAsync(new PaymentPayload
            {
                Amount = 300, CardHolder = "Olawale Lawal", CreditCardNumber = "2648-3783-4934-8938"
            });
            
            Assert.Multiple(() =>
            {
                Assert.NotNull(result);
                Assert.AreEqual(ResponseType.Created, result.ResponseType);
                Assert.NotNull(result.Data);
                Assert.AreEqual(PaymentResult.Processed.ToString(), result.Data.PaymentState);
            });
        }
    }
}