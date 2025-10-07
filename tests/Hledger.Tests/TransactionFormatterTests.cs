using Ledgerly.Api.Common.Data.Entities;
using Ledgerly.Api.Common.Hledger;
using Xunit;

namespace Hledger.Tests;

public class TransactionFormatterTests
{
    private readonly TransactionFormatter _formatter;

    public TransactionFormatterTests()
    {
        _formatter = new TransactionFormatter();
    }

    [Fact]
    public void FormatTransaction_SimpleTransaction_Returns2SpaceIndentation()
    {
        // Arrange
        var transaction = new Transaction
        {
            HledgerTransactionCode = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            Date = new DateTime(2025, 1, 15),
            Payee = "Whole Foods",
            Amount = 45.23m,
            CategoryAccount = "Expenses:Groceries",
            Account = "Assets:Checking"
        };

        // Act
        var result = _formatter.FormatTransaction(transaction);

        // Assert
        var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(3, lines.Length);

        // Line 2 and 3 should start with 2 spaces
        Assert.StartsWith("  ", lines[1]);
        Assert.StartsWith("  ", lines[2]);
    }

    [Fact]
    public void FormatTransaction_WithMemo_IncludesMemoInOutput()
    {
        // Arrange
        var transaction = new Transaction
        {
            HledgerTransactionCode = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            Date = new DateTime(2025, 1, 15),
            Payee = "Whole Foods",
            Amount = 45.23m,
            CategoryAccount = "Expenses:Groceries",
            Account = "Assets:Checking",
            Memo = "Weekly grocery shopping"
        };

        // Act
        var result = _formatter.FormatTransaction(transaction);

        // Assert
        Assert.Contains("| Weekly grocery shopping", result);
    }

    [Fact]
    public void FormatTransaction_DecimalAmount_AlignsAtColumn52()
    {
        // Arrange
        var transaction = new Transaction
        {
            HledgerTransactionCode = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            Date = new DateTime(2025, 1, 15),
            Payee = "Amazon",
            Amount = 123.45m,
            CategoryAccount = "Expenses:Shopping",
            Account = "Assets:Checking"
        };

        // Act
        var result = _formatter.FormatTransaction(transaction);

        // Assert
        var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var amountLine = lines[1];

        // Column 52 alignment: "  Expenses:Shopping" (21 chars) + spaces + "$123.45"
        // Amount should appear around column 52
        var amountIndex = amountLine.IndexOf("$123.45", StringComparison.Ordinal);
        Assert.True(amountIndex >= 45, $"Amount should be aligned near column 52, but found at index {amountIndex}");
    }

    [Fact]
    public void FormatTransaction_Guid_FormatsAsTransactionCode()
    {
        // Arrange
        var guid = Guid.Parse("550e8400-e29b-41d4-a716-446655440000");
        var transaction = new Transaction
        {
            HledgerTransactionCode = guid,
            Date = new DateTime(2025, 1, 15),
            Payee = "Test Payee",
            Amount = 100.00m,
            CategoryAccount = "Expenses:Test",
            Account = "Assets:Checking"
        };

        // Act
        var result = _formatter.FormatTransaction(transaction);

        // Assert
        Assert.Contains($"({guid})", result);
    }

    [Fact]
    public void FormatTransaction_Date_UsesIso8601Format()
    {
        // Arrange
        var transaction = new Transaction
        {
            HledgerTransactionCode = Guid.NewGuid(),
            Date = new DateTime(2025, 1, 15),
            Payee = "Test Payee",
            Amount = 100.00m,
            CategoryAccount = "Expenses:Test",
            Account = "Assets:Checking"
        };

        // Act
        var result = _formatter.FormatTransaction(transaction);

        // Assert
        Assert.StartsWith("2025-01-15", result);
    }

    [Fact]
    public void FormatTransaction_Amount_FormatsWith2Decimals()
    {
        // Arrange
        var transaction = new Transaction
        {
            HledgerTransactionCode = Guid.NewGuid(),
            Date = new DateTime(2025, 1, 15),
            Payee = "Test Payee",
            Amount = 45.2m, // Only 1 decimal place
            CategoryAccount = "Expenses:Test",
            Account = "Assets:Checking"
        };

        // Act
        var result = _formatter.FormatTransaction(transaction);

        // Assert
        Assert.Contains("$45.20", result); // Should format with 2 decimals
    }

    [Fact]
    public void FormatTransaction_NullTransaction_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _formatter.FormatTransaction(null!));
    }

    [Fact]
    public void FormatTransaction_EmptyPayee_ThrowsArgumentException()
    {
        // Arrange
        var transaction = new Transaction
        {
            HledgerTransactionCode = Guid.NewGuid(),
            Date = new DateTime(2025, 1, 15),
            Payee = "",
            Amount = 100.00m,
            CategoryAccount = "Expenses:Test",
            Account = "Assets:Checking"
        };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _formatter.FormatTransaction(transaction));
        Assert.Contains("payee", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatTransaction_EmptyGuid_ThrowsArgumentException()
    {
        // Arrange
        var transaction = new Transaction
        {
            HledgerTransactionCode = Guid.Empty,
            Date = new DateTime(2025, 1, 15),
            Payee = "Test Payee",
            Amount = 100.00m,
            CategoryAccount = "Expenses:Test",
            Account = "Assets:Checking"
        };

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _formatter.FormatTransaction(transaction));
        Assert.Contains("code", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatTransactions_MultipleTransactions_SeparatesWithBlankLines()
    {
        // Arrange
        var transactions = new[]
        {
            new Transaction
            {
                HledgerTransactionCode = Guid.NewGuid(),
                Date = new DateTime(2025, 1, 15),
                Payee = "Payee 1",
                Amount = 100.00m,
                CategoryAccount = "Expenses:Test1",
                Account = "Assets:Checking"
            },
            new Transaction
            {
                HledgerTransactionCode = Guid.NewGuid(),
                Date = new DateTime(2025, 1, 16),
                Payee = "Payee 2",
                Amount = 200.00m,
                CategoryAccount = "Expenses:Test2",
                Account = "Assets:Checking"
            }
        };

        // Act
        var result = _formatter.FormatTransactions(transactions);

        // Assert
        var lines = result.Split('\n');

        // Should have blank line between transactions
        // Transaction 1: 3 lines, blank line, Transaction 2: 3 lines = 7 lines + final newline
        Assert.Contains("Payee 1", result);
        Assert.Contains("Payee 2", result);

        // Check for double newline (blank line separator)
        Assert.Contains("\n\n", result);
    }

    [Fact]
    public void GetAccountsFromTransaction_ReturnsUniqueAccounts()
    {
        // Arrange
        var transaction = new Transaction
        {
            HledgerTransactionCode = Guid.NewGuid(),
            Date = new DateTime(2025, 1, 15),
            Payee = "Test Payee",
            Amount = 100.00m,
            CategoryAccount = "Expenses:Test",
            Account = "Assets:Checking"
        };

        // Act
        var accounts = _formatter.GetAccountsFromTransaction(transaction).ToList();

        // Assert
        Assert.Equal(2, accounts.Count);
        Assert.Contains("Assets:Checking", accounts);
        Assert.Contains("Expenses:Test", accounts);
    }

    [Fact]
    public void FormatTransaction_CompleteExample_MatchesHledgerFormat()
    {
        // Arrange
        var transaction = new Transaction
        {
            HledgerTransactionCode = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            Date = new DateTime(2025, 1, 15),
            Payee = "Whole Foods",
            Amount = 45.23m,
            CategoryAccount = "Expenses:Groceries",
            Account = "Assets:Checking"
        };

        // Act
        var result = _formatter.FormatTransaction(transaction);

        // Assert
        var lines = result.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Line 1: Date (code) Payee
        Assert.Contains("2025-01-15 (550e8400-e29b-41d4-a716-446655440000) Whole Foods", lines[0]);

        // Line 2: Posting with amount
        Assert.Contains("Expenses:Groceries", lines[1]);
        Assert.Contains("$45.23", lines[1]);

        // Line 3: Posting without amount (inferred)
        Assert.Contains("Assets:Checking", lines[2]);
        Assert.DoesNotContain("$", lines[2]);
    }
}
