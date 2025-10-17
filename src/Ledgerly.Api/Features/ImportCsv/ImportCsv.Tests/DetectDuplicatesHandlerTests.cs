using Ledgerly.Api.Common.Data;
using Ledgerly.Api.Common.Data.Entities;
using Ledgerly.Api.Features.ImportCsv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ImportCsv.Tests;

/// <summary>
/// Unit tests for DetectDuplicatesHandler (Story 2.5).
/// Tests transaction hash matching, duplicate detection logic, and batch performance.
/// </summary>
public class DetectDuplicatesHandlerTests : IAsyncLifetime
{
    private readonly LedgerlyDbContext _dbContext;
    private readonly ILogger<DetectDuplicatesHandler> _logger;
    private readonly DetectDuplicatesHandler _sut;

    public DetectDuplicatesHandlerTests()
    {
        _logger = Substitute.For<ILogger<DetectDuplicatesHandler>>();

        // Setup in-memory database for tests
        var options = new DbContextOptionsBuilder<LedgerlyDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;
        _dbContext = new LedgerlyDbContext(options);

        _sut = new DetectDuplicatesHandler(_dbContext, _logger);
    }

    public async Task InitializeAsync()
    {
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task Handle_ExactMatch_ReturnsDuplicate()
    {
        // Arrange
        var existingTransaction = new Transaction
        {
            HledgerTransactionCode = Guid.NewGuid(),
            Date = new DateTime(2025, 1, 15),
            Payee = "Whole Foods",
            Amount = 42.50m,
            Account = "Assets:Checking",
            CategoryAccount = "Expenses:Groceries"
        };
        existingTransaction.UpdateHash();
        _dbContext.Transactions.Add(existingTransaction);
        await _dbContext.SaveChangesAsync();

        var query = new DetectDuplicatesQuery
        {
            Transactions = new List<ParsedTransactionDto>
            {
                new()
                {
                    Date = new DateTime(2025, 1, 15),
                    Payee = "Whole Foods",
                    Amount = 42.50m,
                    RowIndex = 0
                }
            }
        };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Duplicates.Count.ShouldBe(1);
        result.Duplicates[0].TransactionId.ShouldBe(existingTransaction.HledgerTransactionCode);
        result.Duplicates[0].Payee.ShouldBe("Whole Foods");
        result.Duplicates[0].Amount.ShouldBe(42.50m);
        result.Duplicates[0].CsvRowIndex.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_NoMatch_ReturnsEmptyDuplicates()
    {
        // Arrange
        var existingTransaction = new Transaction
        {
            HledgerTransactionCode = Guid.NewGuid(),
            Date = new DateTime(2025, 1, 15),
            Payee = "Whole Foods",
            Amount = 42.50m,
            Account = "Assets:Checking",
            CategoryAccount = "Expenses:Groceries"
        };
        existingTransaction.UpdateHash();
        _dbContext.Transactions.Add(existingTransaction);
        await _dbContext.SaveChangesAsync();

        var query = new DetectDuplicatesQuery
        {
            Transactions = new List<ParsedTransactionDto>
            {
                new()
                {
                    Date = new DateTime(2025, 1, 16), // Different date
                    Payee = "Whole Foods",
                    Amount = 42.50m,
                    RowIndex = 0
                }
            }
        };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Duplicates.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_CaseInsensitivePayeeMatching_ReturnsDuplicate()
    {
        // Arrange
        var existingTransaction = new Transaction
        {
            HledgerTransactionCode = Guid.NewGuid(),
            Date = new DateTime(2025, 1, 15),
            Payee = "WHOLE FOODS", // Uppercase
            Amount = 42.50m,
            Account = "Assets:Checking",
            CategoryAccount = "Expenses:Groceries"
        };
        existingTransaction.UpdateHash();
        _dbContext.Transactions.Add(existingTransaction);
        await _dbContext.SaveChangesAsync();

        var query = new DetectDuplicatesQuery
        {
            Transactions = new List<ParsedTransactionDto>
            {
                new()
                {
                    Date = new DateTime(2025, 1, 15),
                    Payee = "whole foods", // Lowercase
                    Amount = 42.50m,
                    RowIndex = 0
                }
            }
        };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Duplicates.Count.ShouldBe(1);
        result.Duplicates[0].TransactionId.ShouldBe(existingTransaction.HledgerTransactionCode);
    }

    [Fact]
    public async Task Handle_MultipleDuplicates_ReturnsAllMatches()
    {
        // Arrange
        var transactions = new[]
        {
            new Transaction
            {
                HledgerTransactionCode = Guid.NewGuid(),
                Date = new DateTime(2025, 1, 15),
                Payee = "Whole Foods",
                Amount = 42.50m,
                Account = "Assets:Checking",
                CategoryAccount = "Expenses:Groceries"
            },
            new Transaction
            {
                HledgerTransactionCode = Guid.NewGuid(),
                Date = new DateTime(2025, 1, 16),
                Payee = "Amazon",
                Amount = 29.99m,
                Account = "Assets:Checking",
                CategoryAccount = "Expenses:Shopping"
            },
            new Transaction
            {
                HledgerTransactionCode = Guid.NewGuid(),
                Date = new DateTime(2025, 1, 17),
                Payee = "Netflix",
                Amount = 15.99m,
                Account = "Assets:Checking",
                CategoryAccount = "Expenses:Entertainment"
            }
        };

        foreach (var t in transactions)
        {
            t.UpdateHash();
            _dbContext.Transactions.Add(t);
        }
        await _dbContext.SaveChangesAsync();

        var query = new DetectDuplicatesQuery
        {
            Transactions = new List<ParsedTransactionDto>
            {
                new()
                {
                    Date = new DateTime(2025, 1, 15),
                    Payee = "Whole Foods",
                    Amount = 42.50m,
                    RowIndex = 0
                },
                new()
                {
                    Date = new DateTime(2025, 1, 16),
                    Payee = "Amazon",
                    Amount = 29.99m,
                    RowIndex = 1
                },
                new()
                {
                    Date = new DateTime(2025, 1, 18), // Not a duplicate (different date)
                    Payee = "Starbucks",
                    Amount = 5.50m,
                    RowIndex = 2
                }
            }
        };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Duplicates.Count.ShouldBe(2);
        result.Duplicates.Any(d => d.Payee == "Whole Foods").ShouldBeTrue();
        result.Duplicates.Any(d => d.Payee == "Amazon").ShouldBeTrue();
        result.Duplicates.Any(d => d.Payee == "Starbucks").ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_PartialMatch_DifferentAmount_ReturnsNoDuplicate()
    {
        // Arrange
        var existingTransaction = new Transaction
        {
            HledgerTransactionCode = Guid.NewGuid(),
            Date = new DateTime(2025, 1, 15),
            Payee = "Whole Foods",
            Amount = 42.50m,
            Account = "Assets:Checking",
            CategoryAccount = "Expenses:Groceries"
        };
        existingTransaction.UpdateHash();
        _dbContext.Transactions.Add(existingTransaction);
        await _dbContext.SaveChangesAsync();

        var query = new DetectDuplicatesQuery
        {
            Transactions = new List<ParsedTransactionDto>
            {
                new()
                {
                    Date = new DateTime(2025, 1, 15),
                    Payee = "Whole Foods",
                    Amount = 50.00m, // Different amount
                    RowIndex = 0
                }
            }
        };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Duplicates.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_EmptyDatabase_ReturnsNoDuplicates()
    {
        // Arrange
        var query = new DetectDuplicatesQuery
        {
            Transactions = new List<ParsedTransactionDto>
            {
                new()
                {
                    Date = new DateTime(2025, 1, 15),
                    Payee = "Whole Foods",
                    Amount = 42.50m,
                    RowIndex = 0
                }
            }
        };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Duplicates.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_EmptyTransactionList_ReturnsNoDuplicates()
    {
        // Arrange
        var query = new DetectDuplicatesQuery
        {
            Transactions = new List<ParsedTransactionDto>()
        };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Duplicates.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_BatchProcessing_PerformanceOptimization()
    {
        // Arrange - Create 50 existing transactions
        for (int i = 0; i < 50; i++)
        {
            var transaction = new Transaction
            {
                HledgerTransactionCode = Guid.NewGuid(),
                Date = new DateTime(2025, 1, 1).AddDays(i),
                Payee = $"Merchant{i}",
                Amount = 10.00m + i,
                Account = "Assets:Checking",
                CategoryAccount = "Expenses:Shopping"
            };
            transaction.UpdateHash();
            _dbContext.Transactions.Add(transaction);
        }
        await _dbContext.SaveChangesAsync();

        // Check 100 transactions (50 duplicates, 50 new)
        var transactions = new List<ParsedTransactionDto>();
        for (int i = 0; i < 100; i++)
        {
            transactions.Add(new ParsedTransactionDto
            {
                Date = new DateTime(2025, 1, 1).AddDays(i),
                Payee = $"Merchant{i}",
                Amount = 10.00m + i,
                RowIndex = i
            });
        }

        var query = new DetectDuplicatesQuery
        {
            Transactions = transactions
        };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Duplicates.Count.ShouldBe(50);
        result.Duplicates.All(d => d.TransactionId != Guid.Empty).ShouldBeTrue();
    }
}
