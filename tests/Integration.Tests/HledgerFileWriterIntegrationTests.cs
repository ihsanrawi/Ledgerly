using Ledgerly.Api.Common.Data.Entities;
using Ledgerly.Api.Common.Exceptions;
using Ledgerly.Api.Common.Hledger;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Integration.Tests;

public class HledgerFileWriterIntegrationTests : IntegrationTestBase
{
    private readonly HledgerFileWriter _writer;
    private readonly HledgerProcessRunner _processRunner;
    private readonly TransactionFormatter _formatter;
    private readonly ILogger<HledgerBinaryManager> _binaryLogger;
    private readonly ILogger<HledgerProcessRunner> _processLogger;
    private readonly ILogger<HledgerFileWriter> _writerLogger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HledgerFileWriterIntegrationTests()
    {
        _binaryLogger = Substitute.For<ILogger<HledgerBinaryManager>>();
        _processLogger = Substitute.For<ILogger<HledgerProcessRunner>>();
        _writerLogger = Substitute.For<ILogger<HledgerFileWriter>>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();

        _formatter = new TransactionFormatter();
        var binaryManager = new HledgerBinaryManager(_binaryLogger, _httpContextAccessor);
        _processRunner = new HledgerProcessRunner(binaryManager, _processLogger, _httpContextAccessor);
        _writer = new HledgerFileWriter(_processRunner, _formatter, _writerLogger);
    }

    [Fact]
    public async Task AppendTransaction_WritesValidTransaction_HledgerCheckPasses()
    {
        // Arrange
        var filePath = GetTempHledgerFilePath();
        var transaction = new Transaction
        {
            HledgerTransactionCode = Guid.NewGuid(),
            Date = new DateTime(2025, 1, 15),
            Payee = "Whole Foods",
            Amount = 45.23m,
            CategoryAccount = "Expenses:Groceries",
            Account = "Assets:Checking"
        };

        // Act
        var hash = await _writer.AppendTransactionAsync(transaction, filePath);

        // Assert
        Assert.NotEmpty(hash);
        Assert.True(File.Exists(filePath));

        // Verify with hledger check
        var validationResult = await _processRunner.ValidateFile(filePath);
        Assert.True(validationResult.IsValid, $"Validation failed: {string.Join(", ", validationResult.Errors)}");
    }

    [Fact]
    public async Task AppendTransaction_ThenReadBack_ContentMatches()
    {
        // Arrange
        var filePath = GetTempHledgerFilePath();
        var transactionCode = Guid.NewGuid();
        var transaction = new Transaction
        {
            HledgerTransactionCode = transactionCode,
            Date = new DateTime(2025, 1, 15),
            Payee = "Amazon",
            Amount = 123.45m,
            CategoryAccount = "Expenses:Shopping",
            Account = "Assets:Checking"
        };

        // Act
        await _writer.AppendTransactionAsync(transaction, filePath);

        // Assert - Read file content directly
        var content = await File.ReadAllTextAsync(filePath);

        Assert.Contains("2025-01-15", content);
        Assert.Contains(transactionCode.ToString(), content);
        Assert.Contains("Amazon", content);
        Assert.Contains("$123.45", content);
        Assert.Contains("Expenses:Shopping", content);
        Assert.Contains("Assets:Checking", content);
    }

    [Fact]
    public async Task AppendTransaction_FileOperationSucceeds_TempFileCleanedUp()
    {
        // Arrange
        var filePath = GetTempHledgerFilePath();

        // Create initial valid content
        var initialContent = @"account Assets:Checking
account Expenses:Test

2025-01-01 (00000000-0000-0000-0000-000000000000) Initial Transaction
  Expenses:Test    $10.00
  Assets:Checking

";
        await File.WriteAllTextAsync(filePath, initialContent);

        var transaction = new Transaction
        {
            HledgerTransactionCode = Guid.NewGuid(),
            Date = new DateTime(2025, 1, 15),
            Payee = "New Transaction",
            Amount = 50.00m,
            CategoryAccount = "Expenses:Test",
            Account = "Assets:Checking"
        };

        // Act
        await _writer.AppendTransactionAsync(transaction, filePath);

        // Assert
        var tempPath = $"{filePath}.tmp";
        Assert.False(File.Exists(tempPath), "Temp file should be cleaned up after successful write");

        // Verify transaction was added
        var finalContent = await File.ReadAllTextAsync(filePath);
        Assert.Contains("Initial Transaction", finalContent);
        Assert.Contains("New Transaction", finalContent);
    }

    [Fact]
    public async Task AppendTransaction_MultipleSequential_AllPersisted()
    {
        // Arrange
        var filePath = GetTempHledgerFilePath();
        var transactions = new[]
        {
            new Transaction
            {
                HledgerTransactionCode = Guid.NewGuid(),
                Date = new DateTime(2025, 1, 15),
                Payee = "Transaction 1",
                Amount = 10.00m,
                CategoryAccount = "Expenses:Test1",
                Account = "Assets:Checking"
            },
            new Transaction
            {
                HledgerTransactionCode = Guid.NewGuid(),
                Date = new DateTime(2025, 1, 16),
                Payee = "Transaction 2",
                Amount = 20.00m,
                CategoryAccount = "Expenses:Test2",
                Account = "Assets:Checking"
            },
            new Transaction
            {
                HledgerTransactionCode = Guid.NewGuid(),
                Date = new DateTime(2025, 1, 17),
                Payee = "Transaction 3",
                Amount = 30.00m,
                CategoryAccount = "Expenses:Test3",
                Account = "Assets:Checking"
            }
        };

        // Act
        foreach (var transaction in transactions)
        {
            await _writer.AppendTransactionAsync(transaction, filePath);
        }

        // Assert
        var content = await File.ReadAllTextAsync(filePath);

        Assert.Contains("Transaction 1", content);
        Assert.Contains("Transaction 2", content);
        Assert.Contains("Transaction 3", content);

        // Verify hledger can parse all transactions
        var validationResult = await _processRunner.ValidateFile(filePath);
        Assert.True(validationResult.IsValid);
    }

    [Fact]
    public async Task AppendTransaction_WithNewAccount_DeclarationAddedAtTop()
    {
        // Arrange
        var filePath = GetTempHledgerFilePath();
        var transaction = new Transaction
        {
            HledgerTransactionCode = Guid.NewGuid(),
            Date = new DateTime(2025, 1, 15),
            Payee = "Test Payee",
            Amount = 100.00m,
            CategoryAccount = "Expenses:NewCategory",
            Account = "Assets:NewAccount"
        };

        // Act
        await _writer.AppendTransactionAsync(transaction, filePath);

        // Assert
        var content = await File.ReadAllTextAsync(filePath);
        var lines = content.Split('\n');

        // Account declarations should be at the top
        var accountDeclarations = lines
            .Where(l => l.StartsWith("account "))
            .ToList();

        Assert.Contains("account Assets:NewAccount", accountDeclarations);
        Assert.Contains("account Expenses:NewCategory", accountDeclarations);

        // Ensure declarations come before transactions
        var firstTransactionLine = Array.FindIndex(lines, l => l.Contains("2025-01-15"));
        var lastDeclarationLine = Array.FindLastIndex(lines, l => l.StartsWith("account "));

        Assert.True(lastDeclarationLine < firstTransactionLine,
            "Account declarations should appear before transactions");
    }

    [Fact]
    public async Task AppendTransaction_CreatesBackupFile_BeforeWrite()
    {
        // Arrange
        var filePath = GetTempHledgerFilePath();
        var backupPath = $"{filePath}.bak";

        // Create initial file
        var initialContent = @"account Assets:Checking

2025-01-01 (00000000-0000-0000-0000-000000000000) Initial
  Expenses:Test    $5.00
  Assets:Checking

";
        await File.WriteAllTextAsync(filePath, initialContent);

        var transaction = new Transaction
        {
            HledgerTransactionCode = Guid.NewGuid(),
            Date = new DateTime(2025, 1, 15),
            Payee = "New Transaction",
            Amount = 50.00m,
            CategoryAccount = "Expenses:Test",
            Account = "Assets:Checking"
        };

        // Act
        await _writer.AppendTransactionAsync(transaction, filePath);

        // Assert
        Assert.True(File.Exists(backupPath), "Backup file should exist");

        var backupContent = await File.ReadAllTextAsync(backupPath);
        Assert.Contains("Initial", backupContent);
        Assert.DoesNotContain("New Transaction", backupContent);
    }

    [Fact]
    public async Task AppendTransaction_WithMemo_WritesCorrectly()
    {
        // Arrange
        var filePath = GetTempHledgerFilePath();
        var transaction = new Transaction
        {
            HledgerTransactionCode = Guid.NewGuid(),
            Date = new DateTime(2025, 1, 15),
            Payee = "Test Store",
            Amount = 75.50m,
            CategoryAccount = "Expenses:Shopping",
            Account = "Assets:Checking",
            Memo = "Weekly shopping trip"
        };

        // Act
        await _writer.AppendTransactionAsync(transaction, filePath);

        // Assert
        var content = await File.ReadAllTextAsync(filePath);
        Assert.Contains("| Weekly shopping trip", content);

        // Verify hledger can still parse it
        var validationResult = await _processRunner.ValidateFile(filePath);
        Assert.True(validationResult.IsValid);
    }

    private string GetTempHledgerFilePath()
    {
        return Path.Combine(TestDataDirectory, $"test_{Guid.NewGuid()}.hledger");
    }
}
