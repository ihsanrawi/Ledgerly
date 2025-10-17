using Ledgerly.Api.Common.Data;
using Ledgerly.Api.Common.Data.Entities;
using Ledgerly.Api.Common.Exceptions;
using Ledgerly.Api.Common.Hledger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Ledgerly.Api.Features.ImportCsv.ImportCsv.Tests;

/// <summary>
/// Unit tests for ConfirmImportHandler (Story 2.6).
/// Tests import confirmation, transaction persistence, hledger writing, and audit logging.
/// </summary>
public class ConfirmImportHandlerTests : IDisposable
{
    private readonly LedgerlyDbContext _dbContext;
    private readonly IHledgerFileWriter _mockFileWriter;
    private readonly ILogger<ConfirmImportHandler> _mockLogger;
    private readonly ConfirmImportHandler _handler;
    private readonly string _testHledgerFilePath;

    public ConfirmImportHandlerTests()
    {
        // Create in-memory database for testing
        var options = new DbContextOptionsBuilder<LedgerlyDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new LedgerlyDbContext(options);

        // Create mocks - use interface substitution to avoid constructor issues
        _mockFileWriter = Substitute.For<IHledgerFileWriter>();

        _mockLogger = Substitute.For<ILogger<ConfirmImportHandler>>();

        _handler = new ConfirmImportHandler(_dbContext, _mockFileWriter, _mockLogger);

        // Create test .hledger file
        _testHledgerFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".ledgerly",
            "ledger.hledger");

        Directory.CreateDirectory(Path.GetDirectoryName(_testHledgerFilePath)!);
        if (!File.Exists(_testHledgerFilePath))
        {
            File.WriteAllText(_testHledgerFilePath, "");
        }
    }

    [Fact]
    public async Task HandleAsync_SuccessfulImport_SavesTransactionsToDatabase()
    {
        // Arrange
        var command = new ConfirmImportCommand
        {
            FileName = "test.csv",
            UserId = Guid.NewGuid(),
            Transactions = new List<ImportTransactionDto>
            {
                new()
                {
                    Date = DateTime.Parse("2025-01-15"),
                    Payee = "Whole Foods",
                    Amount = 45.23m,
                    Category = "Expenses:Groceries",
                    Account = "Assets:Checking",
                    Memo = "Weekly groceries",
                    IsDuplicate = false
                },
                new()
                {
                    Date = DateTime.Parse("2025-01-16"),
                    Payee = "Amazon",
                    Amount = 89.99m,
                    Category = "Expenses:Shopping",
                    Account = "Assets:Checking",
                    Memo = null,
                    IsDuplicate = false
                }
            }
        };

        // Mock successful file write
        _mockFileWriter.BulkAppendAsync(
            Arg.Any<List<Transaction>>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(new BulkWriteResult
            {
                Success = true,
                FileHashBefore = "hash_before",
                FileHashAfter = "hash_after",
                TransactionsWritten = 2
            });

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, result.TransactionsImported);
        Assert.Equal(0, result.DuplicatesSkipped);
        Assert.Equal(2, result.TransactionIds.Count);

        // Verify transactions saved to database
        var savedTransactions = await _dbContext.Transactions.ToListAsync();
        Assert.Equal(2, savedTransactions.Count);
        Assert.Equal("Whole Foods", savedTransactions[0].Payee);
        Assert.Equal(45.23m, savedTransactions[0].Amount);
        Assert.Equal("Amazon", savedTransactions[1].Payee);
        Assert.Equal(89.99m, savedTransactions[1].Amount);
    }

    [Fact]
    public async Task HandleAsync_FiltersDuplicates_SkipsDuplicateTransactions()
    {
        // Arrange
        var command = new ConfirmImportCommand
        {
            FileName = "test.csv",
            UserId = Guid.NewGuid(),
            Transactions = new List<ImportTransactionDto>
            {
                new()
                {
                    Date = DateTime.Parse("2025-01-15"),
                    Payee = "Whole Foods",
                    Amount = 45.23m,
                    Category = "Expenses:Groceries",
                    Account = "Assets:Checking",
                    IsDuplicate = false
                },
                new()
                {
                    Date = DateTime.Parse("2025-01-16"),
                    Payee = "Amazon",
                    Amount = 89.99m,
                    Category = "Expenses:Shopping",
                    Account = "Assets:Checking",
                    IsDuplicate = true // Duplicate
                }
            }
        };

        _mockFileWriter.BulkAppendAsync(
            Arg.Any<List<Transaction>>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(new BulkWriteResult
            {
                Success = true,
                FileHashBefore = "hash_before",
                FileHashAfter = "hash_after",
                TransactionsWritten = 1
            });

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.TransactionsImported);
        Assert.Equal(1, result.DuplicatesSkipped);

        // Verify only non-duplicate saved
        var savedTransactions = await _dbContext.Transactions.ToListAsync();
        Assert.Single(savedTransactions);
        Assert.Equal("Whole Foods", savedTransactions[0].Payee);
    }

    [Fact]
    public async Task HandleAsync_HledgerValidationFailure_ThrowsException()
    {
        // Arrange
        var command = new ConfirmImportCommand
        {
            FileName = "test.csv",
            UserId = Guid.NewGuid(),
            Transactions = new List<ImportTransactionDto>
            {
                new()
                {
                    Date = DateTime.Parse("2025-01-15"),
                    Payee = "Test Payee",
                    Amount = 100.00m,
                    Category = "Expenses:Test",
                    Account = "Assets:Checking",
                    IsDuplicate = false
                }
            }
        };

        // Mock hledger validation failure
        _mockFileWriter.BulkAppendAsync(
            Arg.Any<List<Transaction>>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Throws(new HledgerValidationException(
                "Validation failed",
                "Transaction unbalanced",
                "/tmp/test.hledger.tmp"));

        // Act & Assert
        await Assert.ThrowsAsync<HledgerException>(
            async () => await _handler.HandleAsync(command));

        // Verify no transactions saved due to rollback
        var savedTransactions = await _dbContext.Transactions.ToListAsync();
        Assert.Empty(savedTransactions);
    }

    [Fact]
    public async Task HandleAsync_CreatesAuditRecords_CsvImportAndFileAudit()
    {
        // Arrange
        var command = new ConfirmImportCommand
        {
            FileName = "bank_transactions.csv",
            UserId = Guid.NewGuid(),
            Transactions = new List<ImportTransactionDto>
            {
                new()
                {
                    Date = DateTime.Parse("2025-01-15"),
                    Payee = "Store",
                    Amount = 50.00m,
                    Category = "Expenses:Shopping",
                    Account = "Assets:Checking",
                    IsDuplicate = false
                }
            }
        };

        _mockFileWriter.BulkAppendAsync(
            Arg.Any<List<Transaction>>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(new BulkWriteResult
            {
                Success = true,
                FileHashBefore = "abc123",
                FileHashAfter = "def456",
                TransactionsWritten = 1
            });

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.True(result.Success);

        // Verify CsvImport audit record
        var csvImport = await _dbContext.CsvImports.FirstOrDefaultAsync();
        Assert.NotNull(csvImport);
        Assert.Equal("bank_transactions.csv", csvImport.FileName);
        Assert.Equal(1, csvImport.SuccessfulImports);
        Assert.Equal(0, csvImport.DuplicatesSkipped);

        // Verify HledgerFileAudit record
        var fileAudit = await _dbContext.HledgerFileAudits.FirstOrDefaultAsync();
        Assert.NotNull(fileAudit);
        Assert.Equal(AuditOperation.CsvImport, fileAudit.Operation);
        Assert.Equal("abc123", fileAudit.FileHashBefore);
        Assert.Equal("def456", fileAudit.FileHashAfter);
        Assert.Equal(1, fileAudit.TransactionCount);
    }

    [Fact]
    public async Task HandleAsync_ComputesTransactionHash_ForDuplicateDetection()
    {
        // Arrange
        var command = new ConfirmImportCommand
        {
            FileName = "test.csv",
            UserId = Guid.NewGuid(),
            Transactions = new List<ImportTransactionDto>
            {
                new()
                {
                    Date = DateTime.Parse("2025-01-15"),
                    Payee = "Whole Foods",
                    Amount = 45.23m,
                    Category = "Expenses:Groceries",
                    Account = "Assets:Checking",
                    IsDuplicate = false
                }
            }
        };

        _mockFileWriter.BulkAppendAsync(
            Arg.Any<List<Transaction>>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(new BulkWriteResult
            {
                Success = true,
                FileHashBefore = "hash_before",
                FileHashAfter = "hash_after",
                TransactionsWritten = 1
            });

        // Act
        await _handler.HandleAsync(command);

        // Assert
        var savedTransaction = await _dbContext.Transactions.FirstAsync();
        Assert.NotEmpty(savedTransaction.Hash);

        // Hash should be consistent with transaction key
        var expectedHash = Transaction.ComputeTransactionHash(
            DateTime.Parse("2025-01-15"),
            "Whole Foods",
            45.23m);
        Assert.Equal(expectedHash, savedTransaction.Hash);
    }

    [Fact]
    public async Task HandleAsync_SanitizesPayee_RemovesSpecialCharacters()
    {
        // Arrange
        var command = new ConfirmImportCommand
        {
            FileName = "test.csv",
            UserId = Guid.NewGuid(),
            Transactions = new List<ImportTransactionDto>
            {
                new()
                {
                    Date = DateTime.Parse("2025-01-15"),
                    Payee = "Test\nPayee\r\nWith\tSpecialChars",
                    Amount = 100.00m,
                    Category = "Expenses:Test",
                    Account = "Assets:Checking",
                    IsDuplicate = false
                }
            }
        };

        _mockFileWriter.BulkAppendAsync(
            Arg.Any<List<Transaction>>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(new BulkWriteResult
            {
                Success = true,
                FileHashBefore = "hash_before",
                FileHashAfter = "hash_after",
                TransactionsWritten = 1
            });

        // Act
        await _handler.HandleAsync(command);

        // Assert
        var savedTransaction = await _dbContext.Transactions.FirstAsync();
        Assert.DoesNotContain("\n", savedTransaction.Payee);
        Assert.DoesNotContain("\r", savedTransaction.Payee);
        Assert.DoesNotContain("\t", savedTransaction.Payee);
    }

    [Fact]
    public async Task HandleAsync_AllDuplicates_ReturnsErrorResponse()
    {
        // Arrange
        var command = new ConfirmImportCommand
        {
            FileName = "test.csv",
            UserId = Guid.NewGuid(),
            Transactions = new List<ImportTransactionDto>
            {
                new() { Date = DateTime.Today, Payee = "Test", Amount = 1, Category = "Test", Account = "Test", IsDuplicate = true },
                new() { Date = DateTime.Today, Payee = "Test2", Amount = 2, Category = "Test", Account = "Test", IsDuplicate = true }
            }
        };

        // Act
        var result = await _handler.HandleAsync(command);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(0, result.TransactionsImported);
        Assert.Equal(2, result.DuplicatesSkipped);
        Assert.Contains("No transactions to import", result.ErrorMessage);
    }

    public void Dispose()
    {
        _dbContext?.Dispose();
    }
}
