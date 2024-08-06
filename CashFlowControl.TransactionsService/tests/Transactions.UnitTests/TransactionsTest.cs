using Xunit;
using CashFlowControl.Application.Services;
using CashFlowControl.Core.Entities;
using CashFlowControl.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashFlowControl.Tests
{
    public class TransactionServiceTests
    {
        private DbContextOptions<CashFlowContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<CashFlowContext>()
                .UseInMemoryDatabase(databaseName: "CashFlowTestDatabase_" + System.Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task AddTransaction_ShouldAddTransaction()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using (var context = new CashFlowContext(options))
            {
                var service = new TransactionService(context);
                var transaction = new Transaction { Id = 1, Amount = 100, Description = "Test case" };

                // Act
                await service.AddTransaction(transaction);
            }

            // Assert
            using (var assertContext = new CashFlowContext(options))
            {
                var addedTransaction = await assertContext.Transactions.FindAsync(1);
                Assert.NotNull(addedTransaction);
                Assert.Equal(1, addedTransaction.Id);
                Assert.Equal(100, addedTransaction.Amount);
            }
        }

        [Fact]
        public async Task GetTransactions_ShouldReturnAllTransactions()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using (var context = new CashFlowContext(options))
            {
                await context.Transactions.AddRangeAsync(
                    new Transaction { Id = 1, Amount = 100, Description = "Test case" },
                    new Transaction { Id = 2, Amount = 200, Description = "Test case" }
                );
                await context.SaveChangesAsync();
            }

            using (var context = new CashFlowContext(options))
            {
                var service = new TransactionService(context);

                // Act
                var result = await service.GetTransactions();

                // Assert
                Assert.Equal(2, ((List<Transaction>)result).Count);
            }
        }

        [Fact]
        public async Task GetTransactionById_ShouldReturnTransaction()
        {
            // Arrange
            var options = CreateNewContextOptions();
            using (var context = new CashFlowContext(options))
            {
                var transaction = new Transaction { Id = 1, Amount = 100, Description = "Test case" };
                await context.Transactions.AddAsync(transaction);
                await context.SaveChangesAsync();
            }

            using (var context = new CashFlowContext(options))
            {
                var service = new TransactionService(context);

                // Act
                var result = await service.GetTransactionById(1);

                // Assert
                Assert.NotNull(result);
                Assert.Equal(1, result.Id);
                Assert.Equal(100, result.Amount);
            }
        }
    }
}
