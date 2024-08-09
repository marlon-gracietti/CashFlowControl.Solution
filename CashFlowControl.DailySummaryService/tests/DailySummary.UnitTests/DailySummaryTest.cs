using Xunit;
using CashFlowControl.Application.Services;
using CashFlowControl.Core.Entities;
using CashFlowControl.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashFlowControl.Tests
{
    public class DailySummaryServiceTests
    {
        private DbContextOptions<CashFlowContext> CreateNewContextOptions()
        {
            return new DbContextOptionsBuilder<CashFlowContext>()
                .UseInMemoryDatabase(databaseName: "CashFlowTestDatabase_" + Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetDailySummary_ShouldReturnCachedSummary_WhenAvailable()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var date = new DateTime(2024, 8, 5);

            using (var context = new CashFlowContext(options))
            {
                context.DailySummaries.Add(new DailySummary
                {
                    Date = date.AddDays(-1),
                    TotalCredits = 100,
                    TotalDebits = 50,
                    Balance = 50
                });

                context.Transactions.AddRange(new List<Transaction>
                {
                    new Transaction { Id = 1, Date = date, Amount = 30, IsCredit = true, Description = "Credit Transaction" },
                    new Transaction { Id = 2, Date = date, Amount = 10, IsCredit = false, Description = "Debit Transaction" }
                });

                await context.SaveChangesAsync();
            }

            using (var context = new CashFlowContext(options))
            {
                var service = new DailySummaryService(context);

                // Act
                var result = await service.GetDailySummary(date);

                // Assert
                Assert.Equal(130, result.TotalCredits);
                Assert.Equal(60, result.TotalDebits);
                Assert.Equal(70, result.Balance);
            }
        }

        [Fact]
        public async Task GetDailySummary_ShouldCalculateFromScratch_WhenNoCachedSummary()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var date = new DateTime(2024, 8, 5);

            using (var context = new CashFlowContext(options))
            {
                context.Transactions.AddRange(new List<Transaction>
                {
                    new Transaction { Id = 1, Date = date.AddDays(-1), Amount = 100, IsCredit = true, Description = "Credit Transaction" },
                    new Transaction { Id = 2, Date = date.AddDays(-1), Amount = 50, IsCredit = false, Description = "Debit Transaction" },
                    new Transaction { Id = 3, Date = date, Amount = 30, IsCredit = true, Description = "Credit Transaction" },
                    new Transaction { Id = 4, Date = date, Amount = 10, IsCredit = false, Description = "Debit Transaction" }
                });

                await context.SaveChangesAsync();
            }

            using (var context = new CashFlowContext(options))
            {
                var service = new DailySummaryService(context);

                // Act
                var result = await service.GetDailySummary(date);

                // Assert
                Assert.Equal(130, result.TotalCredits); // 100 (previous day) + 30 (today)
                Assert.Equal(60, result.TotalDebits); // 50 (previous day) + 10 (today)
                Assert.Equal(70, result.Balance); // 50 (previous day) + 30 (today credits) - 10 (today debits)
            }
        }

        [Fact]
        public async Task GetDailySummary_ShouldReturnCorrectValues_WhenMultipleTransactions()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var date = new DateTime(2024, 8, 5);

            using (var context = new CashFlowContext(options))
            {
                context.Transactions.AddRange(new List<Transaction>
                {
                    new Transaction { Id = 1, Date = date, Amount = 100, IsCredit = true, Description = "Credit Transaction" },
                    new Transaction { Id = 2, Date = date, Amount = 50, IsCredit = false, Description = "Debit Transaction" },
                    new Transaction { Id = 3, Date = date, Amount = 20, IsCredit = true, Description = "Credit Transaction" },
                    new Transaction { Id = 4, Date = date, Amount = 30, IsCredit = false, Description = "Debit Transaction" }
                });

                await context.SaveChangesAsync();
            }

            using (var context = new CashFlowContext(options))
            {
                var service = new DailySummaryService(context);

                // Act
                var result = await service.GetDailySummary(date);

                // Assert
                Assert.Equal(120, result.TotalCredits); // 100 + 20
                Assert.Equal(80, result.TotalDebits); // 50 + 30
                Assert.Equal(40, result.Balance); // 120 - 80
            }
        }

        [Fact]
        public async Task GetDailySummary_ShouldCacheSummary_WhenCalculatedFromScratch()
        {
            // Arrange
            var options = CreateNewContextOptions();
            var date = new DateTime(2024, 8, 5);

            using (var context = new CashFlowContext(options))
            {
                context.Transactions.AddRange(new List<Transaction>
                {
                    new Transaction { Id = 1, Date = date.AddDays(-1), Amount = 100, IsCredit = true, Description = "Credit Transaction" },
                    new Transaction { Id = 2, Date = date.AddDays(-1), Amount = 50, IsCredit = false, Description = "Debit Transaction" },
                    new Transaction { Id = 3, Date = date, Amount = 30, IsCredit = true, Description = "Credit Transaction" },
                    new Transaction { Id = 4, Date = date, Amount = 10, IsCredit = false, Description = "Debit Transaction" }
                });

                await context.SaveChangesAsync();
            }

            using (var context = new CashFlowContext(options))
            {
                var service = new DailySummaryService(context);

                // Act
                var result = await service.GetDailySummary(date);

                // Assert
                var cachedSummary = await context.DailySummaries.FirstOrDefaultAsync(ds => ds.Date == date);
                Assert.NotNull(cachedSummary);
                Assert.Equal(130, cachedSummary.TotalCredits); // 100 (previous day) + 30 (today)
                Assert.Equal(60, cachedSummary.TotalDebits); // 50 (previous day) + 10 (today)
                Assert.Equal(70, cachedSummary.Balance); // 50 (previous day) + 30 (today credits) - 10 (today debits)
            }
        }
    }
}
