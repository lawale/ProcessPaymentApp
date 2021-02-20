using System;
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
    public class UnitOfWorkTests
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
        public async Task Test_StartUnitOfWork_NoTransactionStartedEarlier()
        {
            var unitOfWork = container.GetService<IUnitOfWork>();
            Assert.NotNull(unitOfWork);

            var result = await unitOfWork.StartAsync();
            
            Assert.IsTrue(result);
        }
        
        [Test]
        public async Task Test_StartUnitOfWork_TransactionStartedEarlier()
        {
            var unitOfWork = container.GetService<IUnitOfWork>();
            Assert.NotNull(unitOfWork);

            _ = await unitOfWork.StartAsync();

            var result = await unitOfWork.StartAsync(); //Start a second transaction
            
            Assert.IsFalse(result);
        }
        
        [Test]
        public async Task Test_CheckTransactionExists_TransactionStartedEarlier()
        {
            var unitOfWork = container.GetService<IUnitOfWork>();
            Assert.NotNull(unitOfWork);

            _ = await unitOfWork.StartAsync();

            var result = unitOfWork.TransactionExists();
            
            Assert.IsTrue(result);
        }
        
        [Test]
        public void Test_CheckTransactionExists_NoTransactionStartedEarlier()
        {
            var unitOfWork = container.GetService<IUnitOfWork>();
            Assert.NotNull(unitOfWork);

            var result = unitOfWork.TransactionExists();
            
            Assert.IsFalse(result);
        }

        [Test]
        public void Test_CommitAsync_NoTransactionExists()
        {
            var unitOfWork = container.GetService<IUnitOfWork>();
            Assert.NotNull(unitOfWork);

            Assert.CatchAsync<NullReferenceException>(() => unitOfWork.CommitAsync());
        }
        
        [Test]
        public async Task Test_CommitAsync()
        {
            var unitOfWork = container.GetService<IUnitOfWork>();
            Assert.NotNull(unitOfWork);

            var repo = unitOfWork.PaymentRepository;
            Assert.NotNull(repo);

            await unitOfWork.StartAsync();
            
            var payment = await repo.CreateNewAsync(new Payment
            {
                Amount = 3, CardHolder = "Olawale Lawal", CreditCardNumber = "3333-4444-1223-3234",
                State = PaymentResult.Processed, ExpirationDate = DateTime.UtcNow.AddMonths(11), SecurityCode = "332"
            });

            var paymentExists = await appDbContext.Payments.AnyAsync();
            
            Assert.IsFalse(paymentExists);
            Assert.AreEqual(EntityState.Added, appDbContext.Entry(payment).State);

            await unitOfWork.CommitAsync();
            
            paymentExists = await appDbContext.Payments.AnyAsync();
            
            Assert.IsTrue(paymentExists);
            Assert.AreEqual(EntityState.Unchanged, appDbContext.Entry(payment).State);
            Assert.IsFalse(unitOfWork.TransactionExists());
        }

        [Test]
        public void Test_RollbackAsync_NoTransactionExists()
        {
            var unitOfWork = container.GetService<IUnitOfWork>();
            Assert.NotNull(unitOfWork);

            Assert.CatchAsync<NullReferenceException>(() => unitOfWork.RollbackAsync());
        }
        
        [Test]
        public async Task Test_RollbackAsync()
        {
            var unitOfWork = container.GetService<IUnitOfWork>();
            Assert.NotNull(unitOfWork);

            var repo = unitOfWork.PaymentRepository;
            Assert.NotNull(repo);

            await unitOfWork.StartAsync();
            
            var payment = await repo.CreateNewAsync(new Payment
            {
                Amount = 3, CardHolder = "Olawale Lawal", CreditCardNumber = "3333-4444-1223-3234",
                State = PaymentResult.Processed, ExpirationDate = DateTime.UtcNow.AddMonths(11), SecurityCode = "332"
            });

            var paymentExists = await appDbContext.Payments.AnyAsync();
            
            Assert.IsFalse(paymentExists);
            Assert.AreEqual(EntityState.Added, appDbContext.Entry(payment).State);

            await unitOfWork.RollbackAsync();
            
            paymentExists = await appDbContext.Payments.AnyAsync();
            
            Assert.IsFalse(paymentExists);
            Assert.AreEqual(EntityState.Added, appDbContext.Entry(payment).State);
            Assert.IsFalse(unitOfWork.TransactionExists());
        }
    }
}