using Xunit;

namespace Ledgerly.Api.Features.ImportCsv.Tests;

/// <summary>
/// Unit tests for SaveColumnMappingValidator.
/// Story 2.4 - Manual Column Mapping Interface (SEC-001, DATA-001 mitigation).
/// </summary>
public class SaveColumnMappingValidatorTests
{
    private readonly SaveColumnMappingValidator _validator;

    public SaveColumnMappingValidatorTests()
    {
        _validator = new SaveColumnMappingValidator();
    }

    [Fact]
    public async Task Validate_ValidCommand_ShouldPass()
    {
        // Arrange
        var command = new SaveColumnMappingCommand
        {
            BankIdentifier = "Chase Checking",
            ColumnMappings = new Dictionary<string, string>
            {
                { "Date", "date" },
                { "Amount", "amount" },
                { "Description", "description" }
            },
            HeaderSignature = new[] { "Date", "Amount", "Description" }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task Validate_EmptyBankIdentifier_ShouldFail()
    {
        // Arrange
        var command = new SaveColumnMappingCommand
        {
            BankIdentifier = "",
            ColumnMappings = new Dictionary<string, string> { { "Date", "date" }, { "Amount", "amount" } },
            HeaderSignature = new[] { "Date", "Amount" }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "BankIdentifier");
    }

    [Fact]
    public async Task Validate_BankIdentifierTooLong_ShouldFail()
    {
        // Arrange
        var command = new SaveColumnMappingCommand
        {
            BankIdentifier = new string('A', 101), // 101 characters
            ColumnMappings = new Dictionary<string, string> { { "Date", "date" }, { "Amount", "amount" } },
            HeaderSignature = new[] { "Date", "Amount" }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "BankIdentifier" && e.ErrorMessage.Contains("100 characters"));
    }

    [Theory]
    [InlineData("Test<script>alert('xss')</script>")]
    [InlineData("Test=SUM(A1:A10)")]
    [InlineData("Test@IMPORT")]
    [InlineData("Test|rm -rf")]
    public async Task Validate_BankIdentifierWithInvalidCharacters_ShouldFail(string invalidIdentifier)
    {
        // Arrange - SEC-001 mitigation
        var command = new SaveColumnMappingCommand
        {
            BankIdentifier = invalidIdentifier,
            ColumnMappings = new Dictionary<string, string> { { "Date", "date" }, { "Amount", "amount" } },
            HeaderSignature = new[] { "Date", "Amount" }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "BankIdentifier");
    }

    [Fact]
    public async Task Validate_ValidBankIdentifierWithSpacesAndHyphens_ShouldPass()
    {
        // Arrange
        var command = new SaveColumnMappingCommand
        {
            BankIdentifier = "Chase Bank-Checking Account_001",
            ColumnMappings = new Dictionary<string, string> { { "Date", "date" }, { "Amount", "amount" } },
            HeaderSignature = new[] { "Date", "Amount" }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_EmptyColumnMappings_ShouldFail()
    {
        // Arrange - DATA-001 mitigation
        var command = new SaveColumnMappingCommand
        {
            BankIdentifier = "Test Bank",
            ColumnMappings = new Dictionary<string, string>(),
            HeaderSignature = new[] { "Date", "Amount" }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ColumnMappings");
    }

    [Fact]
    public async Task Validate_MissingDateField_ShouldFail()
    {
        // Arrange - DATA-001 mitigation
        var command = new SaveColumnMappingCommand
        {
            BankIdentifier = "Test Bank",
            ColumnMappings = new Dictionary<string, string>
            {
                { "Amount", "amount" },
                { "Description", "description" }
            },
            HeaderSignature = new[] { "Amount", "Description" }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("date"));
    }

    [Fact]
    public async Task Validate_MissingAmountField_ShouldFail()
    {
        // Arrange - DATA-001 mitigation
        var command = new SaveColumnMappingCommand
        {
            BankIdentifier = "Test Bank",
            ColumnMappings = new Dictionary<string, string>
            {
                { "Date", "date" },
                { "Description", "description" }
            },
            HeaderSignature = new[] { "Date", "Description" }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("amount"));
    }

    [Fact]
    public async Task Validate_WithDebitCredit_ShouldPass()
    {
        // Arrange - Split debit/credit columns should be accepted
        var command = new SaveColumnMappingCommand
        {
            BankIdentifier = "Test Bank",
            ColumnMappings = new Dictionary<string, string>
            {
                { "Date", "date" },
                { "Debit", "debit" },
                { "Credit", "credit" },
                { "Description", "description" }
            },
            HeaderSignature = new[] { "Date", "Debit", "Credit", "Description" }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithOnlyDebit_ShouldPass()
    {
        // Arrange
        var command = new SaveColumnMappingCommand
        {
            BankIdentifier = "Test Bank",
            ColumnMappings = new Dictionary<string, string>
            {
                { "Date", "date" },
                { "Debit", "debit" }
            },
            HeaderSignature = new[] { "Date", "Debit" }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_InvalidFieldType_ShouldFail()
    {
        // Arrange
        var command = new SaveColumnMappingCommand
        {
            BankIdentifier = "Test Bank",
            ColumnMappings = new Dictionary<string, string>
            {
                { "Date", "date" },
                { "Amount", "amount" },
                { "Invalid", "invalid_field_type" }
            },
            HeaderSignature = new[] { "Date", "Amount", "Invalid" }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.ErrorMessage.Contains("valid field types"));
    }

    [Fact]
    public async Task Validate_EmptyHeaderSignature_ShouldFail()
    {
        // Arrange - TECH-001 mitigation
        var command = new SaveColumnMappingCommand
        {
            BankIdentifier = "Test Bank",
            ColumnMappings = new Dictionary<string, string> { { "Date", "date" }, { "Amount", "amount" } },
            HeaderSignature = Array.Empty<string>()
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "HeaderSignature");
    }

    [Fact]
    public async Task Validate_AllValidFieldTypes_ShouldPass()
    {
        // Arrange
        var command = new SaveColumnMappingCommand
        {
            BankIdentifier = "Test Bank",
            ColumnMappings = new Dictionary<string, string>
            {
                { "Date", "date" },
                { "Amount", "amount" },
                { "Description", "description" },
                { "Memo", "memo" },
                { "Balance", "balance" },
                { "Account", "account" },
                { "Debit", "debit" },
                { "Credit", "credit" }
            },
            HeaderSignature = new[] { "Date", "Amount", "Description", "Memo", "Balance", "Account", "Debit", "Credit" }
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        Assert.True(result.IsValid);
    }
}
