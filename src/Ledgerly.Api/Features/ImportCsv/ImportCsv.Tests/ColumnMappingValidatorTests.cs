using Ledgerly.Api.Features.ImportCsv;
using Ledgerly.Contracts.Dtos;
using Xunit;

namespace Ledgerly.Api.Features.ImportCsv.Tests;

/// <summary>
/// Unit tests for ColumnMappingValidator.
/// Tests validation of column detection results.
/// </summary>
public class ColumnMappingValidatorTests
{
    private readonly ColumnMappingValidator _validator;

    public ColumnMappingValidatorTests()
    {
        _validator = new ColumnMappingValidator();
    }

    [Fact]
    public void Validate_WithAllRequiredFieldsDetected_IsValid()
    {
        // Arrange
        var result = new ColumnDetectionResult
        {
            DetectedMappings = new Dictionary<string, string>
            {
                ["Date"] = "date",
                ["Amount"] = "amount"
            },
            ConfidenceScores = new Dictionary<string, decimal>
            {
                ["date"] = 0.95m,
                ["amount"] = 0.90m
            },
            AllRequiredFieldsDetected = true
        };

        // Act
        var validationResult = _validator.Validate(result);

        // Assert
        Assert.True(validationResult.IsValid);
    }

    [Fact]
    public void Validate_WithoutDateColumn_IsInvalid()
    {
        // Arrange
        var result = new ColumnDetectionResult
        {
            DetectedMappings = new Dictionary<string, string>
            {
                ["Amount"] = "amount"
            },
            ConfidenceScores = new Dictionary<string, decimal>
            {
                ["amount"] = 0.90m
            },
            AllRequiredFieldsDetected = false
        };

        // Act
        var validationResult = _validator.Validate(result);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Errors, e => e.ErrorMessage.Contains("Date column"));
    }

    [Fact]
    public void Validate_WithoutAmountColumn_IsInvalid()
    {
        // Arrange
        var result = new ColumnDetectionResult
        {
            DetectedMappings = new Dictionary<string, string>
            {
                ["Date"] = "date"
            },
            ConfidenceScores = new Dictionary<string, decimal>
            {
                ["date"] = 0.95m
            },
            AllRequiredFieldsDetected = false
        };

        // Act
        var validationResult = _validator.Validate(result);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Errors, e => e.ErrorMessage.Contains("Amount column"));
    }

    [Fact]
    public void Validate_WithLowDateConfidence_IsInvalid()
    {
        // Arrange
        var result = new ColumnDetectionResult
        {
            DetectedMappings = new Dictionary<string, string>
            {
                ["Date"] = "date",
                ["Amount"] = "amount"
            },
            ConfidenceScores = new Dictionary<string, decimal>
            {
                ["date"] = 0.5m, // Below threshold
                ["amount"] = 0.90m
            },
            AllRequiredFieldsDetected = false
        };

        // Act
        var validationResult = _validator.Validate(result);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Errors, e => e.ErrorMessage.Contains("Date column"));
    }

    [Fact]
    public void Validate_WithDebitCreditColumns_IsValid()
    {
        // Arrange
        var result = new ColumnDetectionResult
        {
            DetectedMappings = new Dictionary<string, string>
            {
                ["Date"] = "date",
                ["Debit"] = "debit",
                ["Credit"] = "credit"
            },
            ConfidenceScores = new Dictionary<string, decimal>
            {
                ["date"] = 0.95m,
                ["amount"] = 0.85m, // Combined from debit/credit
                ["debit"] = 0.90m,
                ["credit"] = 0.80m
            },
            AllRequiredFieldsDetected = true
        };

        // Act
        var validationResult = _validator.Validate(result);

        // Assert
        Assert.True(validationResult.IsValid);
    }
}
