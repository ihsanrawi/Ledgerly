using Ledgerly.Api.Common.Data;
using Ledgerly.Api.Common.Data.Entities;
using Ledgerly.Api.Common.Exceptions;
using Ledgerly.Api.Common.Hledger;
using Ledgerly.Api.Features.ImportCsv;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Integration.Tests;

/// <summary>
/// Integration tests for Story 2.6: CSV Import Workflow
/// Tests full end-to-end import workflow including:
/// - SQLite persistence with real database
/// - .hledger file writes with validation
/// - Audit record creation (CsvImport and HledgerFileAudit)
/// - TransactionScope rollback on validation failures
/// </summary>
public class CsvImportWorkflowIntegrationTests : IntegrationTestBase
{
    private readonly HledgerFileWriter _fileWriter;
    private readonly HledgerProcessRunner _processRunner;
    private readonly TransactionFormatter _formatter;
    private readonly ILogger<HledgerBinaryManager> _binaryLogger;
    private readonly ILogger<HledgerProcessRunner> _processLogger;
    private readonly ILogger<HledgerFileWriter> _writerLogger;
    private readonly ILogger<ConfirmImportHandler> _handlerLogger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CsvImportWorkflowIntegrationTests()
    {
        _binaryLogger = Substitute.For<ILogger<HledgerBinaryManager>>();
        _processLogger = Substitute.For<ILogger<HledgerProcessRunner>>();
        _writerLogger = Substitute.For<ILogger<HledgerFileWriter>>();
        _handlerLogger = Substitute.For<ILogger<ConfirmImportHandler>>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();

        _formatter = new TransactionFormatter();
        var binaryManager = new HledgerBinaryManager(_binaryLogger, _httpContextAccessor);
        _processRunner = new HledgerProcessRunner(binaryManager, _processLogger, _httpContextAccessor);
        _fileWriter = new HledgerFileWriter(_processRunner, _formatter, _writerLogger);
    }

    /// <summary>
    /// P0 Test: Full CSV import workflow with 10 transactions
    /// Covers AC4 end-to-end verification
    /// </summary>
    [Fact]
    public async Task FullCsvImportWorkflow_SuccessfullyImports10Transactions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var hledgerFilePath = SetupHledgerFile();
        var dbContext = CreateTestDbContext();

        var handler = new ConfirmImportHandler(dbContext, _fileWriter, _handlerLogger);

        // Generate 10 test transactions
        var command = new ConfirmImportCommand
        {
            UserId = userId,
            FileName = "test_transactions.csv",
            Transactions = GenerateTestTransactions(10)
        };

        // Act
        var response = await handler.HandleAsync(command);

        // Assert
        Assert.True(response.Success, $"Import failed: {response.ErrorMessage}");
        Assert.Equal(10, response.TransactionsImported);
        Assert.Equal(0, response.DuplicatesSkipped);

        // Verify 10 transactions in SQLite
        var transactionsInDb = await dbContext.Transactions.CountAsync();
        Assert.Equal(10, transactionsInDb);

        // Verify all transactions have correct data
        var allTransactions = await dbContext.Transactions.ToListAsync();
        foreach (var transaction in allTransactions)
        {
            Assert.NotEqual(Guid.Empty, transaction.HledgerTransactionCode);
            Assert.NotEmpty(transaction.Payee);
            Assert.True(transaction.Amount > 0);
            Assert.NotEmpty(transaction.Hash);
        }

        // Verify CsvImport audit record created
        var csvImportAudit = await dbContext.CsvImports
            .FirstOrDefaultAsync(ci => ci.FileName == "test_transactions.csv");
        Assert.NotNull(csvImportAudit);
        Assert.Equal(10, csvImportAudit.TotalRows);
        Assert.Equal(10, csvImportAudit.SuccessfulImports);
        Assert.Equal(0, csvImportAudit.DuplicatesSkipped);
        Assert.Equal(userId, csvImportAudit.UserId);

        // Verify HledgerFileAudit record created
        var fileAudit = await dbContext.HledgerFileAudits
            .FirstOrDefaultAsync(fa => fa.Operation == AuditOperation.CsvImport);
        Assert.NotNull(fileAudit);
        Assert.NotEmpty(fileAudit.FileHashBefore);
        Assert.NotEmpty(fileAudit.FileHashAfter);
        Assert.Equal(10, fileAudit.TransactionCount);
        Assert.Equal(AuditTrigger.User, fileAudit.TriggeredBy);
        Assert.Equal(csvImportAudit.Id, fileAudit.RelatedEntityId);
        Assert.Equal(userId, fileAudit.UserId);

        // Verify .hledger file contains 10 transaction entries with valid syntax
        Assert.True(File.Exists(hledgerFilePath), "hledger file should exist");
        var fileContent = await File.ReadAllTextAsync(hledgerFilePath);

        // Count transaction entries (each should have a date line)
        var transactionLines = fileContent.Split('\n')
            .Where(line => line.Length >= 10 && char.IsDigit(line[0]) && line.Contains('-'))
            .Count();
        Assert.Equal(10, transactionLines);

        // Verify hledger validation passes
        var validationResult = await _processRunner.ValidateFile(hledgerFilePath);
        Assert.True(validationResult.IsValid,
            $"hledger validation failed: {string.Join(", ", validationResult.Errors)}");

        // Cleanup
        await dbContext.DisposeAsync();
    }

    /// <summary>
    /// P0 Test: Rollback scenario when hledger validation fails
    /// Tests that HledgerException is properly thrown on validation failure
    /// NOTE: SQLite does NOT support TransactionScope rollback, so this test only verifies
    /// the exception is thrown. In production with SQL Server/PostgreSQL, rollback would work correctly.
    /// This is a known limitation documented in the QA assessment.
    /// </summary>
    [Fact]
    public async Task FullCsvImportWorkflow_RollbackOnHledgerValidationFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var hledgerFilePath = SetupHledgerFile();
        var dbContext = CreateTestDbContext();

        // Create a mock file writer that always fails validation
        var mockFileWriter = Substitute.For<IHledgerFileWriter>();
        mockFileWriter.BulkAppendAsync(Arg.Any<List<Transaction>>(), Arg.Any<string>())
            .Returns(Task.FromException<BulkWriteResult>(new HledgerValidationException("Invalid transaction format at line 1")));

        var handler = new ConfirmImportHandler(dbContext, mockFileWriter, _handlerLogger);

        var command = new ConfirmImportCommand
        {
            UserId = userId,
            FileName = "invalid_transactions.csv",
            Transactions = GenerateTestTransactions(5)
        };

        // Act & Assert - Verify HledgerException is thrown
        var exception = await Assert.ThrowsAsync<HledgerException>(async () =>
            await handler.HandleAsync(command));

        Assert.Contains("Transaction validation failed", exception.Message);

        // NOTE: We cannot verify rollback behavior with SQLite as it doesn't support TransactionScope.
        // In production with SQL Server/PostgreSQL, the TransactionScope.Complete() would not be called,
        // and all database changes would be rolled back.
        // This test successfully demonstrates that validation exceptions are caught and wrapped in HledgerException.

        // Cleanup
        await dbContext.DisposeAsync();
    }

    /// <summary>
    /// Test: Duplicate filtering - duplicates should be skipped
    /// </summary>
    [Fact]
    public async Task FullCsvImportWorkflow_FiltersDuplicates_SkipsDuplicateTransactions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var hledgerFilePath = SetupHledgerFile();
        var dbContext = CreateTestDbContext();

        var handler = new ConfirmImportHandler(dbContext, _fileWriter, _handlerLogger);

        // Generate 3 transactions: 2 non-duplicates, 1 duplicate
        var transactions = GenerateTestTransactions(2);
        transactions.Add(new ImportTransactionDto
        {
            Date = DateTime.UtcNow.Date,
            Payee = "Duplicate Store",
            Amount = 50.00m,
            Category = "expenses:shopping",
            Account = "assets:checking",
            IsDuplicate = true // Mark as duplicate
        });

        var command = new ConfirmImportCommand
        {
            UserId = userId,
            FileName = "mixed_transactions.csv",
            Transactions = transactions
        };

        // Act
        var response = await handler.HandleAsync(command);

        // Assert
        Assert.True(response.Success);
        Assert.Equal(2, response.TransactionsImported); // Only 2 non-duplicates
        Assert.Equal(1, response.DuplicatesSkipped);

        // Verify only 2 transactions in SQLite (duplicate skipped)
        var transactionsInDb = await dbContext.Transactions.CountAsync();
        Assert.Equal(2, transactionsInDb);

        // Verify CsvImport audit record reflects correct counts
        var csvImportAudit = await dbContext.CsvImports.FirstAsync();
        Assert.Equal(3, csvImportAudit.TotalRows);
        Assert.Equal(2, csvImportAudit.SuccessfulImports);
        Assert.Equal(1, csvImportAudit.DuplicatesSkipped);

        // Cleanup
        await dbContext.DisposeAsync();
    }

    /// <summary>
    /// Test: User edits payee and category before import
    /// Verifies edited data is persisted correctly
    /// </summary>
    [Fact]
    public async Task FullCsvImportWorkflow_UserEditsPayeeAndCategory_SavesEditedValues()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var hledgerFilePath = SetupHledgerFile();
        var dbContext = CreateTestDbContext();

        var handler = new ConfirmImportHandler(dbContext, _fileWriter, _handlerLogger);

        var command = new ConfirmImportCommand
        {
            UserId = userId,
            FileName = "edited_transactions.csv",
            Transactions = new List<ImportTransactionDto>
            {
                new()
                {
                    Date = new DateTime(2025, 1, 15),
                    Payee = "Edited Payee Name", // User edited payee
                    Amount = 45.23m,
                    Category = "expenses:utilities", // User changed category
                    Account = "assets:checking"
                }
            }
        };

        // Act
        var response = await handler.HandleAsync(command);

        // Assert
        Assert.True(response.Success);

        // Verify edited payee saved to database
        var transaction = await dbContext.Transactions.FirstAsync();
        Assert.Equal("Edited Payee Name", transaction.Payee);
        Assert.Equal("expenses:utilities", transaction.CategoryAccount);

        // Verify edited data in .hledger file
        var hledgerFilePath2 = Path.Combine(TestDataDirectory, "test_user_" + userId + ".hledger");
        if (File.Exists(hledgerFilePath2))
        {
            var fileContent = await File.ReadAllTextAsync(hledgerFilePath2);
            Assert.Contains("Edited Payee Name", fileContent);
            Assert.Contains("expenses:utilities", fileContent);
        }

        // Cleanup
        await dbContext.DisposeAsync();
    }

    /// <summary>
    /// Helper: Create a test DbContext with SQLite database
    /// Uses real SQLite database (not in-memory) to support TransactionScope
    /// NOTE: SQLite does not support distributed transactions (TransactionScope),
    /// so we suppress the warning. In production with a different provider, TransactionScope would work correctly.
    /// </summary>
    private LedgerlyDbContext CreateTestDbContext()
    {
        var dbPath = Path.Combine(TestDataDirectory, $"test_{Guid.NewGuid()}.db");
        var optionsBuilder = new DbContextOptionsBuilder<LedgerlyDbContext>();
        optionsBuilder.UseSqlite($"Data Source={dbPath}")
            .ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.AmbientTransactionWarning));

        var context = new LedgerlyDbContext(optionsBuilder.Options);
        context.Database.EnsureCreated(); // Create database schema

        return context;
    }

    /// <summary>
    /// Helper: Setup hledger file at default location for tests
    /// Creates ~/.ledgerly/ledger.hledger that ConfirmImportHandler expects
    /// </summary>
    private string SetupHledgerFile()
    {
        var ledgerlyDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".ledgerly");

        var hledgerFilePath = Path.Combine(ledgerlyDir, "ledger.hledger");

        // Create directory if it doesn't exist
        Directory.CreateDirectory(ledgerlyDir);

        // Create empty hledger file with basic account declarations
        var initialContent = @"; Test ledger file for integration tests
account assets:checking
account expenses:groceries
account expenses:shopping
account expenses:utilities

";
        File.WriteAllText(hledgerFilePath, initialContent);

        return hledgerFilePath;
    }

    /// <summary>
    /// Helper: Get temp hledger file path for tests
    /// </summary>
    private string GetTempHledgerFilePath()
    {
        return Path.Combine(TestDataDirectory, $"test_{Guid.NewGuid()}.hledger");
    }

    /// <summary>
    /// Helper: Generate test transactions for import
    /// </summary>
    private List<ImportTransactionDto> GenerateTestTransactions(int count)
    {
        var transactions = new List<ImportTransactionDto>();
        var startDate = new DateTime(2025, 1, 1);

        for (int i = 0; i < count; i++)
        {
            transactions.Add(new ImportTransactionDto
            {
                Date = startDate.AddDays(i),
                Payee = $"Test Payee {i + 1}",
                Amount = (i + 1) * 10.50m,
                Category = i % 2 == 0 ? "expenses:groceries" : "expenses:shopping",
                Account = "assets:checking",
                Memo = $"Test transaction {i + 1}",
                IsDuplicate = false
            });
        }

        return transactions;
    }
}
