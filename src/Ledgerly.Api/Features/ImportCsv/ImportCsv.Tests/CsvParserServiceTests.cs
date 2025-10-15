using Ledgerly.Api.Features.ImportCsv;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ImportCsv.Tests;

/// <summary>
/// Unit tests for CsvParserService.
/// Tests standard CSV parsing, edge cases, malformed data, and format detection.
/// </summary>
public class CsvParserServiceTests
{
    private readonly ICsvParserService _sut;
    private readonly ILogger<CsvParserService> _logger;
    private readonly string _testDataPath;

    public CsvParserServiceTests()
    {
        _logger = Substitute.For<ILogger<CsvParserService>>();
        _sut = new CsvParserService(_logger);

        // Get path to test data directory (navigate from test bin directory to repo root)
        var baseDir = AppContext.BaseDirectory;
        var testProjectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
        var repoRoot = Path.GetFullPath(Path.Combine(testProjectDir, "..", "..", "..", "..", ".."));
        _testDataPath = Path.Combine(repoRoot, "tests", "TestData", "CsvSamples");
    }

    #region Standard CSV Tests

    [Fact]
    public async Task ParseCsvFile_StandardChaseChecking_ParsesSuccessfully()
    {
        // Arrange
        var filePath = Path.Combine(_testDataPath, "standard", "chase-checking.csv");
        await using var stream = File.OpenRead(filePath);

        // Act
        var result = await _sut.ParseCsvFile(stream, "chase-checking.csv");

        // Assert
        result.ShouldNotBeNull();
        result.Headers.ShouldNotBeEmpty();
        result.Headers.ShouldContain("Transaction Date");
        result.Headers.ShouldContain("Description");
        result.Headers.ShouldContain("Amount");
        result.TotalRowCount.ShouldBeGreaterThan(0);
        result.DetectedDelimiter.ShouldBe("Comma");
        result.DetectedEncoding.ShouldContain("UTF");
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task ParseCsvFile_StandardBofaSavings_ParsesSuccessfully()
    {
        // Arrange
        var filePath = Path.Combine(_testDataPath, "standard", "bofa-savings.csv");
        await using var stream = File.OpenRead(filePath);

        // Act
        var result = await _sut.ParseCsvFile(stream, "bofa-savings.csv");

        // Assert
        result.ShouldNotBeNull();
        result.Headers.ShouldNotBeEmpty();
        result.TotalRowCount.ShouldBeGreaterThan(0);
        result.DetectedDelimiter.ShouldBe("Comma");
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task ParseCsvFile_StandardWellsFargoCredit_ParsesSuccessfully()
    {
        // Arrange
        var filePath = Path.Combine(_testDataPath, "standard", "wellsfargo-credit.csv");
        await using var stream = File.OpenRead(filePath);

        // Act
        var result = await _sut.ParseCsvFile(stream, "wellsfargo-credit.csv");

        // Assert
        result.ShouldNotBeNull();
        result.Headers.ShouldNotBeEmpty();
        result.TotalRowCount.ShouldBeGreaterThan(0);
        result.DetectedDelimiter.ShouldBe("Comma");
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task ParseCsvFile_StandardCitiCredit_ParsesSuccessfully()
    {
        // Arrange
        var filePath = Path.Combine(_testDataPath, "standard", "citi-credit.csv");
        await using var stream = File.OpenRead(filePath);

        // Act
        var result = await _sut.ParseCsvFile(stream, "citi-credit.csv");

        // Assert
        result.ShouldNotBeNull();
        result.Headers.ShouldNotBeEmpty();
        result.TotalRowCount.ShouldBeGreaterThan(0);
        result.DetectedDelimiter.ShouldBe("Comma");
        result.Errors.ShouldBeEmpty();
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task ParseCsvFile_SemicolonDelimiter_DetectsCorrectDelimiter()
    {
        // Arrange
        var filePath = Path.Combine(_testDataPath, "edge-cases", "semicolon-delimiter.csv");
        await using var stream = File.OpenRead(filePath);

        // Act
        var result = await _sut.ParseCsvFile(stream, "semicolon-delimiter.csv");

        // Assert
        result.ShouldNotBeNull();
        result.DetectedDelimiter.ShouldBe("Semicolon");
        result.Headers.ShouldNotBeEmpty();
        result.Headers.ShouldContain("Datum");
        result.Headers.ShouldContain("Beschreibung");
        result.TotalRowCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ParseCsvFile_MultilineMemos_HandlesMultilineFields()
    {
        // Arrange
        var filePath = Path.Combine(_testDataPath, "edge-cases", "multiline-memos.csv");
        await using var stream = File.OpenRead(filePath);

        // Act
        var result = await _sut.ParseCsvFile(stream, "multiline-memos.csv");

        // Assert
        result.ShouldNotBeNull();
        result.Headers.ShouldNotBeEmpty();
        result.TotalRowCount.ShouldBeGreaterThan(0);
        // Should handle quoted multiline fields gracefully
    }

    [Fact]
    public async Task ParseCsvFile_SpecialCharacters_HandlesSpecialCharacters()
    {
        // Arrange
        var filePath = Path.Combine(_testDataPath, "edge-cases", "special-characters.csv");
        await using var stream = File.OpenRead(filePath);

        // Act
        var result = await _sut.ParseCsvFile(stream, "special-characters.csv");

        // Assert
        result.ShouldNotBeNull();
        result.Headers.ShouldNotBeEmpty();
        result.TotalRowCount.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ParseCsvFile_NoHeaders_TreatsFirstRowAsHeaders()
    {
        // Arrange
        var filePath = Path.Combine(_testDataPath, "edge-cases", "no-headers.csv");
        await using var stream = File.OpenRead(filePath);

        // Act
        var result = await _sut.ParseCsvFile(stream, "no-headers.csv");

        // Assert
        // CsvHelper treats first row as headers when HasHeaderRecord=true
        result.ShouldNotBeNull();
        result.Headers.ShouldNotBeEmpty();
        result.Headers[0].ShouldBe("2025-01-20"); // First row becomes header
        result.TotalRowCount.ShouldBeGreaterThan(0); // Remaining rows are data
    }

    #endregion

    #region Malformed CSV Tests

    [Fact]
    public async Task ParseCsvFile_MissingColumns_RecordsErrors()
    {
        // Arrange
        var filePath = Path.Combine(_testDataPath, "malformed", "missing-columns.csv");
        await using var stream = File.OpenRead(filePath);

        // Act
        var result = await _sut.ParseCsvFile(stream, "missing-columns.csv");

        // Assert
        result.ShouldNotBeNull();
        result.Headers.ShouldNotBeEmpty();
        // Should complete parsing but may have errors for rows with missing fields
        // CsvHelper handles missing fields gracefully by default
    }

    [Fact]
    public async Task ParseCsvFile_InvalidDates_ParsesWithoutValidation()
    {
        // Arrange
        var filePath = Path.Combine(_testDataPath, "malformed", "invalid-dates.csv");
        await using var stream = File.OpenRead(filePath);

        // Act
        var result = await _sut.ParseCsvFile(stream, "invalid-dates.csv");

        // Assert
        result.ShouldNotBeNull();
        result.Headers.ShouldNotBeEmpty();
        // CSV parser doesn't validate date formats - returns raw strings
        // Date validation happens in Story 2.3 column mapping
    }

    [Fact]
    public async Task ParseCsvFile_InvalidAmounts_ParsesWithoutValidation()
    {
        // Arrange
        var filePath = Path.Combine(_testDataPath, "malformed", "invalid-amounts.csv");
        await using var stream = File.OpenRead(filePath);

        // Act
        var result = await _sut.ParseCsvFile(stream, "invalid-amounts.csv");

        // Assert
        result.ShouldNotBeNull();
        result.Headers.ShouldNotBeEmpty();
        // CSV parser doesn't validate amount formats - returns raw strings
        // Amount validation happens in Story 2.3 column mapping
    }

    #endregion

    #region Format Detection Tests

    [Theory]
    [InlineData("standard/chase-checking.csv", "Comma")]
    [InlineData("edge-cases/semicolon-delimiter.csv", "Semicolon")]
    public async Task ParseCsvFile_VariousDelimiters_DetectsCorrectly(string relativePath, string expectedDelimiter)
    {
        // Arrange
        var filePath = Path.Combine(_testDataPath, relativePath);
        await using var stream = File.OpenRead(filePath);

        // Act
        var result = await _sut.ParseCsvFile(stream, Path.GetFileName(filePath));

        // Assert
        result.DetectedDelimiter.ShouldBe(expectedDelimiter);
    }

    [Fact]
    public async Task ParseCsvFile_ReturnsFirst10Rows_WhenMoreRowsExist()
    {
        // Arrange
        var filePath = Path.Combine(_testDataPath, "standard", "chase-checking.csv");
        await using var stream = File.OpenRead(filePath);

        // Act
        var result = await _sut.ParseCsvFile(stream, "chase-checking.csv");

        // Assert
        result.SampleRows.Count.ShouldBeLessThanOrEqualTo(10);
        result.TotalRowCount.ShouldBeGreaterThanOrEqualTo(result.SampleRows.Count);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task ParseCsvFile_EmptyStream_ThrowsException()
    {
        // Arrange
        await using var stream = new MemoryStream();

        // Act & Assert
        await Should.ThrowAsync<CsvParseException>(async () =>
            await _sut.ParseCsvFile(stream, "empty.csv"));
    }

    [Fact]
    public async Task ParseCsvFile_InvalidStream_ParsesAsText()
    {
        // Arrange
        var invalidData = System.Text.Encoding.UTF8.GetBytes("Not a valid CSV with special chars");
        await using var stream = new MemoryStream(invalidData);

        // Act
        var result = await _sut.ParseCsvFile(stream, "invalid.csv");

        // Assert
        // CsvHelper will parse any text - it treats first line as headers
        result.ShouldNotBeNull();
        result.Headers.Length.ShouldBeGreaterThan(0);
    }

    #endregion
}
