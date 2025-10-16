using Ledgerly.Api.Features.ImportCsv;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ledgerly.Api.Features.ImportCsv.Tests;

/// <summary>
/// Unit tests for ColumnDetectionService.
/// Tests column detection accuracy for date, amount, description, memo, and balance columns.
/// </summary>
public class ColumnDetectionServiceTests
{
    private readonly IColumnDetectionService _service;
    private readonly ILogger<ColumnDetectionService> _logger;

    public ColumnDetectionServiceTests()
    {
        _logger = Substitute.For<ILogger<ColumnDetectionService>>();
        _service = new ColumnDetectionService(_logger);
    }

    [Fact]
    public async Task DetectColumns_WithStandardHeaders_DetectsAllRequiredFields()
    {
        // Arrange
        var headers = new[] { "Date", "Description", "Amount", "Balance" };
        var sampleRows = new List<Dictionary<string, string>>
        {
            new() { ["Date"] = "2025-01-15", ["Description"] = "Coffee Shop", ["Amount"] = "-5.75", ["Balance"] = "1250.00" },
            new() { ["Date"] = "2025-01-14", ["Description"] = "Paycheck", ["Amount"] = "2500.00", ["Balance"] = "1255.75" }
        };

        // Act
        var result = await _service.DetectColumns(headers, sampleRows);

        // Assert
        Assert.True(result.AllRequiredFieldsDetected);
        Assert.Contains("Date", result.DetectedMappings.Keys);
        Assert.Equal("date", result.DetectedMappings["Date"]);
        Assert.Contains("Description", result.DetectedMappings.Keys);
        Assert.Equal("description", result.DetectedMappings["Description"]);
        Assert.Contains("Amount", result.DetectedMappings.Keys);
        Assert.Equal("amount", result.DetectedMappings["Amount"]);
        Assert.Contains("Balance", result.DetectedMappings.Keys);
        Assert.Equal("balance", result.DetectedMappings["Balance"]);

        // Check confidence scores
        Assert.True(result.ConfidenceScores["date"] >= 0.7m);
        Assert.True(result.ConfidenceScores["amount"] >= 0.7m);
    }

    [Fact]
    public async Task DetectColumns_WithUSDateFormat_DetectsDateColumn()
    {
        // Arrange
        var headers = new[] { "Transaction Date", "Payee", "Amount" };
        var sampleRows = new List<Dictionary<string, string>>
        {
            new() { ["Transaction Date"] = "01/15/2025", ["Payee"] = "Coffee Shop", ["Amount"] = "-5.75" },
            new() { ["Transaction Date"] = "1/14/2025", ["Payee"] = "Gas Station", ["Amount"] = "-45.00" }
        };

        // Act
        var result = await _service.DetectColumns(headers, sampleRows);

        // Assert
        Assert.Contains("Transaction Date", result.DetectedMappings.Keys);
        Assert.Equal("date", result.DetectedMappings["Transaction Date"]);
        Assert.True(result.ConfidenceScores["date"] >= 0.9m);
    }

    [Fact]
    public async Task DetectColumns_WithEuropeanDateFormat_DetectsDateColumn()
    {
        // Arrange
        var headers = new[] { "Datum", "Beschreibung", "Betrag" };
        var sampleRows = new List<Dictionary<string, string>>
        {
            new() { ["Datum"] = "15.01.2025", ["Beschreibung"] = "Café", ["Betrag"] = "-5,75" },
            new() { ["Datum"] = "14.01.2025", ["Beschreibung"] = "Supermarkt", ["Betrag"] = "-45,00" }
        };

        // Act
        var result = await _service.DetectColumns(headers, sampleRows);

        // Assert
        Assert.Contains("Datum", result.DetectedMappings.Keys);
        Assert.Equal("date", result.DetectedMappings["Datum"]);
        Assert.True(result.ConfidenceScores["date"] >= 0.8m); // German header has 0.85 base confidence
    }

    [Fact]
    public async Task DetectColumns_WithSplitDebitCredit_DetectsBothColumns()
    {
        // Arrange
        var headers = new[] { "Date", "Description", "Debit", "Credit", "Balance" };
        var sampleRows = new List<Dictionary<string, string>>
        {
            new() { ["Date"] = "2025-01-15", ["Description"] = "Purchase", ["Debit"] = "50.00", ["Credit"] = "", ["Balance"] = "1234.56" },
            new() { ["Date"] = "2025-01-12", ["Description"] = "Payment", ["Debit"] = "", ["Credit"] = "100.00", ["Balance"] = "1284.56" }
        };

        // Act
        var result = await _service.DetectColumns(headers, sampleRows);

        // Assert
        Assert.Contains("Debit", result.DetectedMappings.Keys);
        Assert.Equal("debit", result.DetectedMappings["Debit"]);
        Assert.Contains("Credit", result.DetectedMappings.Keys);
        Assert.Equal("credit", result.DetectedMappings["Credit"]);
        Assert.True(result.ConfidenceScores["amount"] >= 0.7m); // Combined confidence from debit/credit
    }

    [Fact]
    public async Task DetectColumns_WithParenthesesNegatives_DetectsAmountColumn()
    {
        // Arrange
        var headers = new[] { "Date", "Merchant", "Amount" };
        var sampleRows = new List<Dictionary<string, string>>
        {
            new() { ["Date"] = "2025-01-15", ["Merchant"] = "Restaurant", ["Amount"] = "(50.00)" },
            new() { ["Date"] = "2025-01-12", ["Merchant"] = "Store", ["Amount"] = "100.00" }
        };

        // Act
        var result = await _service.DetectColumns(headers, sampleRows);

        // Assert
        Assert.Contains("Amount", result.DetectedMappings.Keys);
        Assert.Equal("amount", result.DetectedMappings["Amount"]);
        Assert.True(result.ConfidenceScores["amount"] >= 0.9m);
    }

    [Fact]
    public async Task DetectColumns_WithCurrencySymbols_DetectsAmountColumn()
    {
        // Arrange
        var headers = new[] { "Date", "Description", "Transaction Amount" };
        var sampleRows = new List<Dictionary<string, string>>
        {
            new() { ["Date"] = "2025-01-15", ["Description"] = "Purchase", ["Transaction Amount"] = "$50.00" },
            new() { ["Date"] = "2025-01-12", ["Description"] = "Payment", ["Transaction Amount"] = "$100.00" }
        };

        // Act
        var result = await _service.DetectColumns(headers, sampleRows);

        // Assert
        Assert.Contains("Transaction Amount", result.DetectedMappings.Keys);
        Assert.Equal("amount", result.DetectedMappings["Transaction Amount"]);
        Assert.True(result.ConfidenceScores["amount"] >= 0.9m);
    }

    [Fact]
    public async Task DetectColumns_WithMemoColumn_DetectsDescriptionAndMemo()
    {
        // Arrange
        var headers = new[] { "Date", "Payee", "Amount", "Memo" };
        var sampleRows = new List<Dictionary<string, string>>
        {
            new() { ["Date"] = "2025-01-15", ["Payee"] = "ACME Corp", ["Amount"] = "1500.00", ["Memo"] = "Invoice #12345" },
            new() { ["Date"] = "2025-01-12", ["Payee"] = "Office Supplies", ["Amount"] = "250.00", ["Memo"] = "Quarterly order" }
        };

        // Act
        var result = await _service.DetectColumns(headers, sampleRows);

        // Assert
        Assert.Contains("Payee", result.DetectedMappings.Keys);
        Assert.Equal("description", result.DetectedMappings["Payee"]);
        Assert.Contains("Memo", result.DetectedMappings.Keys);
        Assert.Equal("memo", result.DetectedMappings["Memo"]);
    }

    [Fact]
    public async Task DetectColumns_WithLongestTextColumn_FallbackToDescription()
    {
        // Arrange - No standard description headers
        var headers = new[] { "Date", "Name", "Amount", "Code" };
        var sampleRows = new List<Dictionary<string, string>>
        {
            new() { ["Date"] = "2025-01-15", ["Name"] = "ACME Corporation Store Location", ["Amount"] = "50.00", ["Code"] = "PUR" },
            new() { ["Date"] = "2025-01-12", ["Name"] = "XYZ Supermarket Downtown", ["Amount"] = "100.00", ["Code"] = "GRO" }
        };

        // Act
        var result = await _service.DetectColumns(headers, sampleRows);

        // Assert
        Assert.Contains("Name", result.DetectedMappings.Keys);
        Assert.Equal("description", result.DetectedMappings["Name"]);
    }

    [Fact]
    public async Task DetectColumns_WithMissingDateColumn_ReturnsLowConfidence()
    {
        // Arrange - No date column
        var headers = new[] { "Payee", "Amount", "Balance" };
        var sampleRows = new List<Dictionary<string, string>>
        {
            new() { ["Payee"] = "Coffee Shop", ["Amount"] = "-5.75", ["Balance"] = "1250.00" },
            new() { ["Payee"] = "Gas Station", ["Amount"] = "-45.00", ["Balance"] = "1255.75" }
        };

        // Act
        var result = await _service.DetectColumns(headers, sampleRows);

        // Assert
        Assert.False(result.AllRequiredFieldsDetected);
        Assert.Contains("Date column not detected", result.Warnings[0]);
    }

    [Fact]
    public async Task DetectColumns_WithMissingAmountColumn_ReturnsLowConfidence()
    {
        // Arrange - No amount column
        var headers = new[] { "Date", "Description" };
        var sampleRows = new List<Dictionary<string, string>>
        {
            new() { ["Date"] = "2025-01-15", ["Description"] = "Coffee Shop" },
            new() { ["Date"] = "2025-01-14", ["Description"] = "Gas Station" }
        };

        // Act
        var result = await _service.DetectColumns(headers, sampleRows);

        // Assert
        Assert.False(result.AllRequiredFieldsDetected);
        Assert.Contains("Amount column not detected", result.Warnings[0]);
    }

    [Fact]
    public async Task DetectColumns_WithEmptyHeaders_ReturnsEmptyResult()
    {
        // Arrange
        var headers = Array.Empty<string>();
        var sampleRows = new List<Dictionary<string, string>>();

        // Act
        var result = await _service.DetectColumns(headers, sampleRows);

        // Assert
        Assert.Empty(result.DetectedMappings);
        Assert.False(result.AllRequiredFieldsDetected);
    }

    [Fact]
    public async Task DetectColumns_WithInvalidDateData_ReturnsLowDateConfidence()
    {
        // Arrange - Header says "Date" but data is not dates
        var headers = new[] { "Date", "Description", "Amount" };
        var sampleRows = new List<Dictionary<string, string>>
        {
            new() { ["Date"] = "Not a date", ["Description"] = "Coffee Shop", ["Amount"] = "-5.75" },
            new() { ["Date"] = "Also not a date", ["Description"] = "Gas Station", ["Amount"] = "-45.00" }
        };

        // Act
        var result = await _service.DetectColumns(headers, sampleRows);

        // Assert
        // Date header match gives some confidence, but data validation reduces it
        Assert.True(result.ConfidenceScores.GetValueOrDefault("date", 0) < 0.9m);
    }

    [Fact]
    public async Task DetectColumns_WithBalanceColumn_DetectsBalanceField()
    {
        // Arrange
        var headers = new[] { "Date", "Description", "Amount", "Running Balance" };
        var sampleRows = new List<Dictionary<string, string>>
        {
            new() { ["Date"] = "2025-01-15", ["Description"] = "Purchase", ["Amount"] = "-50.00", ["Running Balance"] = "1234.56" },
            new() { ["Date"] = "2025-01-12", ["Description"] = "Deposit", ["Amount"] = "100.00", ["Running Balance"] = "1284.56" }
        };

        // Act
        var result = await _service.DetectColumns(headers, sampleRows);

        // Assert
        Assert.Contains("Running Balance", result.DetectedMappings.Keys);
        Assert.Equal("balance", result.DetectedMappings["Running Balance"]);
        Assert.Contains("balance", result.ConfidenceScores.Keys);
        Assert.True(result.ConfidenceScores["balance"] > 0);
    }

    [Theory]
    [InlineData("Date", "Payee", "Amount")]
    [InlineData("Transaction Date", "Merchant", "Transaction Amount")]
    [InlineData("Post Date", "Description", "Amt")]
    public async Task DetectColumns_WithVariousHeaderCombinations_DetectsAllFields(
        string dateHeader, string descHeader, string amountHeader)
    {
        // Arrange
        var headers = new[] { dateHeader, descHeader, amountHeader };
        var sampleRows = new List<Dictionary<string, string>>
        {
            new() { [dateHeader] = "2025-01-15", [descHeader] = "Coffee Shop", [amountHeader] = "-5.75" },
            new() { [dateHeader] = "2025-01-14", [descHeader] = "Gas Station", [amountHeader] = "-45.00" }
        };

        // Act
        var result = await _service.DetectColumns(headers, sampleRows);

        // Assert
        Assert.True(result.AllRequiredFieldsDetected);
        Assert.Contains("date", result.ConfidenceScores.Keys);
        Assert.Contains("description", result.ConfidenceScores.Keys);
        Assert.Contains("amount", result.ConfidenceScores.Keys);
    }

    [Fact]
    public async Task DetectColumns_HighConfidenceThreshold_ExactMatchAndFullValidation()
    {
        // Arrange - Perfect match scenario
        var headers = new[] { "date", "description", "amount" };
        var sampleRows = new List<Dictionary<string, string>>
        {
            new() { ["date"] = "2025-01-15", ["description"] = "Coffee Shop", ["amount"] = "5.75" },
            new() { ["date"] = "2025-01-14", ["description"] = "Gas Station", ["amount"] = "45.00" },
            new() { ["date"] = "2025-01-13", ["description"] = "Grocery Store", ["amount"] = "125.50" }
        };

        // Act
        var result = await _service.DetectColumns(headers, sampleRows);

        // Assert
        Assert.True(result.ConfidenceScores["date"] >= 0.95m); // High confidence expected
        Assert.True(result.ConfidenceScores["amount"] >= 0.95m);
    }

    [Fact]
    public async Task DetectColumns_MediumConfidenceThreshold_SubstringMatchAndPartialValidation()
    {
        // Arrange - One invalid date in sample
        var headers = new[] { "Trans Date", "Details", "Amt" };
        var sampleRows = new List<Dictionary<string, string>>
        {
            new() { ["Trans Date"] = "2025-01-15", ["Details"] = "Coffee", ["Amt"] = "5.75" },
            new() { ["Trans Date"] = "Invalid", ["Details"] = "Gas", ["Amt"] = "45.00" }
        };

        // Act
        var result = await _service.DetectColumns(headers, sampleRows);

        // Assert
        // Date validation is 50% (1 valid, 1 invalid), header match ~0.85-0.9 -> average ~0.7-0.8
        Assert.True(result.ConfidenceScores["date"] >= 0.7m && result.ConfidenceScores["date"] < 0.95m);
    }
}
