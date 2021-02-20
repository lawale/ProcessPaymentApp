using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using OlawaleFiledApp.Core;
using OlawaleFiledApp.Core.Data;
using OlawaleFiledApp.Core.Data.Repositories;
using OlawaleFiledApp.Core.Domain;

namespace OlawaleFiledApp.Test.RepositoryTests
{
    public class PaymentRepositoryTests
    {
        private ServiceProvider container;
        private AppDbContext appDbContext;

        [SetUp]
        public void Setup()
        {
            var serviceCollection = new ServiceCollection();
            
            serviceCollection.AddLogging();
            serviceCollection.InitCoreServicesAndRepositories();

            serviceCollection.AddDbContext<AppDbContext>(x =>
            {
                var connection = new SqliteConnection($"Data Source={Guid.NewGuid()};Mode=Memory");
                connection.Open();
                x.UseSqlite(connection);
            });
            container = serviceCollection.BuildServiceProvider();

            appDbContext = container.GetService<AppDbContext>();
            Assert.NotNull(appDbContext);
            appDbContext.Database.EnsureCreated();
        }

        [Test]
        public async Task Test_CreateNewAsync()
        {
            var repo = container.GetService<IPaymentRepository>();
            Assert.NotNull(repo);
            var payment = await repo.CreateNewAsync(new Payment
            {
                Amount = 3, CardHolder = "Olawale Lawal", CreditCardNumber = "3333-4444-1223-3234",
                State = PaymentResult.Processed, ExpirationDate = DateTime.UtcNow.AddMonths(11), SecurityCode = "332"
            });

            Assert.Multiple(() =>
            {
                Assert.NotNull(payment);
                Assert.AreNotEqual(default(Guid),payment.Id);
                Assert.AreEqual(EntityState.Added, appDbContext.Entry(payment).State);
            });
            
        }
        
        [Test]
        public async Task Test_UpdateAsync()
        {
            var repo = container.GetService<IPaymentRepository>();
            Assert.NotNull(repo);
            var payment = await repo.CreateNewAsync(new Payment
            {
                Amount = 3, CardHolder = "Olawale Lawal", CreditCardNumber = "3333-4444-1223-3234",
                State = PaymentResult.Processed, ExpirationDate = DateTime.UtcNow.AddMonths(11), SecurityCode = "332"
            });

            await appDbContext.SaveChangesAsync();

            payment.Amount = 2000;
            await repo.UpdateAsync(payment);
            
            Assert.Multiple(() =>
            {
                Assert.NotNull(payment);
                Assert.AreEqual(EntityState.Modified, appDbContext.Entry(payment).State);
            });
        }

        [Test]
        public async Task Test_GetByIdAsync()
        {
            var repo = container.GetService<IPaymentRepository>();
            Assert.NotNull(repo);
            var payment = await repo.CreateNewAsync(new Payment
            {
                Amount = 3, CardHolder = "Olawale Lawal", CreditCardNumber = "3333-4444-1223-3234",
                State = PaymentResult.Processed, ExpirationDate = DateTime.UtcNow.AddMonths(11), SecurityCode = "332"
            });

            await appDbContext.SaveChangesAsync();

            var newPayment = await repo.GetByIdAsync(payment.Id);
            
            Assert.Multiple(() =>
            {
                Assert.NotNull(newPayment);
                Assert.AreEqual(newPayment, payment);
                Assert.AreEqual(EntityState.Unchanged, appDbContext.Entry(payment).State);
                Assert.AreEqual(EntityState.Unchanged, appDbContext.Entry(newPayment).State);
            });
        }

        [Test]
        public async Task Test_DeleteAsync_SoftDeleteEnabled()
        {
            var repo = container.GetService<IPaymentRepository>();
            Assert.NotNull(repo);
            var payment = await repo.CreateNewAsync(new Payment
            {
                Amount = 3, CardHolder = "Olawale Lawal", CreditCardNumber = "3333-4444-1223-3234",
                State = PaymentResult.Processed, ExpirationDate = DateTime.UtcNow.AddMonths(11), SecurityCode = "332"
            });

            await appDbContext.SaveChangesAsync();

            await repo.DeleteAsync(payment, true);
            
            Assert.Multiple(() =>
            {
                Assert.NotNull(payment);
                Assert.IsTrue(payment.IsDeleted);
                Assert.NotNull(payment.DeletedAt);
                Assert.AreEqual(EntityState.Modified, appDbContext.Entry(payment).State);
            });
        }
        
        [Test]
        public async Task Test_DeleteAsync_HardDeleteEnabled()
        {
            var repo = container.GetService<IPaymentRepository>();
            Assert.NotNull(repo);
            var payment = await repo.CreateNewAsync(new Payment
            {
                Amount = 3, CardHolder = "Olawale Lawal", CreditCardNumber = "3333-4444-1223-3234",
                State = PaymentResult.Processed, ExpirationDate = DateTime.UtcNow.AddMonths(11), SecurityCode = "332"
            });

            await appDbContext.SaveChangesAsync();

            await repo.DeleteAsync(payment);
            
            Assert.Multiple(() =>
            {
                Assert.NotNull(payment);
                Assert.IsFalse(payment.IsDeleted);
                Assert.Null(payment.DeletedAt);
                Assert.AreEqual(EntityState.Deleted, appDbContext.Entry(payment).State);
            });
        }

        [Test]
        public async Task Test_GetQuery()
        {
            var repo = container.GetService<IPaymentRepository>();
            Assert.NotNull(repo);
            var payment = await repo.CreateNewAsync(new Payment
            {
                Amount = 3, CardHolder = "Olawale Lawal", CreditCardNumber = "3333-4444-1223-3234",
                State = PaymentResult.Processed, ExpirationDate = DateTime.UtcNow.AddMonths(11), SecurityCode = "332"
            });
            
            var paymentOne = await repo.CreateNewAsync(new Payment
            {
                Amount = 3, CardHolder = "Olawale Lawal", CreditCardNumber = "3333-89785-1223-3234",
                State = PaymentResult.Failed, ExpirationDate = DateTime.UtcNow.AddMonths(11), SecurityCode = "332"
            });

            await appDbContext.SaveChangesAsync();

            var payments = await repo.GetQuery().Where(x => x.State == PaymentResult.Processed).ToListAsync();

            Assert.Multiple(() =>
            {
                Assert.NotNull(payment);
                Assert.NotNull(paymentOne);
                Assert.IsNotEmpty(payments);
                Assert.Contains(payment, payments);
                Assert.Catch(() => Assert.Contains(paymentOne, payments));
                Assert.AreEqual(EntityState.Unchanged, appDbContext.Entry(payment).State);
                Assert.AreEqual(EntityState.Unchanged, appDbContext.Entry(paymentOne).State);
            });
        }
    }
}