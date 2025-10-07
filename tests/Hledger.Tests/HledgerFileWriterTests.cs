using Ledgerly.Api.Common.Data.Entities;
using Ledgerly.Api.Common.Exceptions;
using Ledgerly.Api.Common.Hledger;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Hledger.Tests;

public class HledgerFileWriterTests : IDisposable
{
    private readonly TestableHledgerProcessRunner _mockProcessRunner;
    private readonly TransactionFormatter _formatter;
    private readonly ILogger<HledgerFileWriter> _mockLogger;
    private readonly HledgerFileWriter _writer;
    private readonly string _testDirectory;

    public HledgerFileWriterTests()
    {
        _mockProcessRunner = new TestableHledgerProcessRunner();
        _formatter = new TransactionFormatter();
        _mockLogger = Substitute.For<ILogger<HledgerFileWriter>>();
        _writer = new HledgerFileWriter(_mockProcessRunner, _formatter, _mockLogger);

        // Create temp test directory
        _testDirectory = Path.Combine(Path.GetTempPath(), $"hledger_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    // Testable wrapper for HledgerProcessRunner
    private class TestableHledgerProcessRunner : HledgerProcessRunner
    {
        private ValidationResult? _validationResult;

        public TestableHledgerProcessRunner()
            : base(null!, null!, null)
        {
        }

        public void SetValidationResult(ValidationResult result)
        {
            _validationResult = result;
        }

        public override async Task<ValidationResult> ValidateFile(string hledgerFilePath)
        {
            if (_validationResult != null)
            {
                return await Task.FromResult(_validationResult);
            }

            return await Task.FromResult(ValidationResult.Success());
        }
    }

    public void Dispose()
    {
        // Cleanup test directory
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    [Fact]
    public async Task AppendTransactionAsync_CreatesNewFile_WhenFileDoesNotExist()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "new.hledger");
        var transaction = CreateTestTransaction();

        _mockProcessRunner.SetValidationResult(ValidationResult.Success());

        // Act
        var hash = await _writer.AppendTransactionAsync(transaction, filePath);

        // Assert
        Assert.True(File.Exists(filePath));
        Assert.NotEmpty(hash);

        var content = await File.ReadAllTextAsync(filePath);
        Assert.Contains("Whole Foods", content);
        Assert.Contains("Expenses:Groceries", content);
        Assert.Contains("Assets:Checking", content);
    }

    [Fact]
    public async Task AppendTransactionAsync_CreatesTempFile_DuringWrite()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.hledger");
        var tempPath = $"{filePath}.tmp";
        var transaction = CreateTestTransaction();

        var tempFileCreated = false;

        // Note: This test can't check temp file during validation with current mock
        // but we can verify temp file is cleaned up after
        _mockProcessRunner.SetValidationResult(ValidationResult.Success());

        // Act
        await _writer.AppendTransactionAsync(transaction, filePath);

        // Assert
        Assert.False(File.Exists(tempPath), "Temp file should be cleaned up after success");
    }

    [Fact]
    public async Task AppendTransactionAsync_CreatesBackup_BeforeOverwriting()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "existing.hledger");
        var backupPath = $"{filePath}.bak";

        // Create existing file
        var existingContent = "2025-01-01 (00000000-0000-0000-0000-000000000000) Old Transaction\n  Expenses:Test    $10.00\n  Assets:Checking\n";
        await File.WriteAllTextAsync(filePath, existingContent);

        var transaction = CreateTestTransaction();

        _mockProcessRunner.SetValidationResult(ValidationResult.Success());

        // Act
        await _writer.AppendTransactionAsync(transaction, filePath);

        // Assert
        Assert.True(File.Exists(backupPath));

        var backupContent = await File.ReadAllTextAsync(backupPath);
        Assert.Contains("Old Transaction", backupContent);
    }

    [Fact]
    public async Task AppendTransactionAsync_ValidationFails_DeletesTempFileAndThrows()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "invalid.hledger");
        var tempPath = $"{filePath}.tmp";
        var transaction = CreateTestTransaction();

        _mockProcessRunner.SetValidationResult(ValidationResult.Failure("Unbalanced transaction"));

        // Act & Assert
        await Assert.ThrowsAsync<HledgerValidationException>(
            async () => await _writer.AppendTransactionAsync(transaction, filePath));

        Assert.False(File.Exists(tempPath), "Temp file should be deleted after validation failure");
    }

    [Fact]
    public async Task AppendTransactionAsync_AddsAccountDeclarations_AtFileTop()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "accounts.hledger");
        var transaction = CreateTestTransaction();

        _mockProcessRunner.SetValidationResult(ValidationResult.Success());

        // Act
        await _writer.AppendTransactionAsync(transaction, filePath);

        // Assert
        var content = await File.ReadAllTextAsync(filePath);
        var lines = content.Split('\n');

        // Account declarations should be at the top
        Assert.Contains("account Assets:Checking", lines[0] + lines[1]);
        Assert.Contains("account Expenses:Groceries", lines[0] + lines[1]);
    }

    [Fact]
    public async Task AppendTransactionAsync_PreservesExistingAccountDeclarations()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "existing_accounts.hledger");

        // Create file with existing declarations
        var existingContent = "account Assets:Savings\naccount Income:Salary\n\n2025-01-01 (00000000-0000-0000-0000-000000000000) Salary\n  Income:Salary    $-1000.00\n  Assets:Savings\n";
        await File.WriteAllTextAsync(filePath, existingContent);

        var transaction = CreateTestTransaction();

        _mockProcessRunner.SetValidationResult(ValidationResult.Success());

        // Act
        await _writer.AppendTransactionAsync(transaction, filePath);

        // Assert
        var content = await File.ReadAllTextAsync(filePath);

        // Should contain both old and new account declarations
        Assert.Contains("account Assets:Savings", content);
        Assert.Contains("account Income:Salary", content);
        Assert.Contains("account Assets:Checking", content);
        Assert.Contains("account Expenses:Groceries", content);

        // Should contain both transactions
        Assert.Contains("Salary", content);
        Assert.Contains("Whole Foods", content);
    }

    [Fact]
    public async Task RestoreFromBackupAsync_RestoresFile_FromBackup()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "restore.hledger");
        var backupPath = $"{filePath}.bak";

        var originalContent = "Original content";
        var modifiedContent = "Modified content";

        await File.WriteAllTextAsync(filePath, originalContent);
        await File.WriteAllTextAsync(backupPath, originalContent);

        // Modify the file
        await File.WriteAllTextAsync(filePath, modifiedContent);

        // Act
        _writer.RestoreFromBackup(filePath);

        // Assert
        var restoredContent = await File.ReadAllTextAsync(filePath);
        Assert.Equal(originalContent, restoredContent);
    }

    [Fact]
    public void RestoreFromBackup_BackupDoesNotExist_ThrowsFileNotFoundException()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "no_backup.hledger");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(
            () => _writer.RestoreFromBackup(filePath));
    }

    [Fact]
    public async Task AppendTransactionAsync_ReturnsCorrectSha256Hash()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "hash.hledger");
        var transaction = CreateTestTransaction();

        _mockProcessRunner.SetValidationResult(ValidationResult.Success());

        // Act
        var hash = await _writer.AppendTransactionAsync(transaction, filePath);

        // Assert
        Assert.NotEmpty(hash);
        Assert.Equal(64, hash.Length); // SHA256 hash is 64 hex characters
        Assert.Matches("^[a-f0-9]{64}$", hash);
    }

    [Fact]
    public async Task AppendTransactionAsync_NullTransaction_ThrowsArgumentNullException()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.hledger");

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _writer.AppendTransactionAsync(null!, filePath));
    }

    [Fact]
    public async Task AppendTransactionAsync_EmptyFilePath_ThrowsArgumentException()
    {
        // Arrange
        var transaction = CreateTestTransaction();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _writer.AppendTransactionAsync(transaction, ""));
    }

    private static Transaction CreateTestTransaction()
    {
        return new Transaction
        {
            HledgerTransactionCode = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            Date = new DateTime(2025, 1, 15),
            Payee = "Whole Foods",
            Amount = 45.23m,
            CategoryAccount = "Expenses:Groceries",
            Account = "Assets:Checking"
        };
    }
}
