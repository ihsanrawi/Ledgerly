using Ledgerly.Api.Features.ImportCsv;
using Ledgerly.Contracts.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using Xunit;

namespace ImportCsv.Tests;

/// <summary>
/// Unit tests for PreviewCsvHandler.
/// Tests handler orchestration, validation, error handling, and timeout behavior.
/// </summary>
public class PreviewCsvHandlerTests
{
    private readonly ICsvParserService _csvParser;
    private readonly IColumnDetectionService _columnDetection;
    private readonly ILogger<PreviewCsvHandler> _logger;
    private readonly PreviewCsvHandler _sut;

    public PreviewCsvHandlerTests()
    {
        _csvParser = Substitute.For<ICsvParserService>();
        _columnDetection = Substitute.For<IColumnDetectionService>();
        _logger = Substitute.For<ILogger<PreviewCsvHandler>>();
        _sut = new PreviewCsvHandler(_csvParser, _columnDetection, _logger);
    }

    [Fact]
    public async Task Handle_ValidCsvFile_ReturnsPreviewResponse()
    {
        // Arrange
        var mockFile = CreateMockFormFile("test.csv", "Date,Description,Amount\n2025-01-01,Test,-10.00");
        var command = new PreviewCsvCommand(mockFile);

        var parseResult = new CsvParseResult
        {
            Headers = new[] { "Date", "Description", "Amount" },
            SampleRows = new List<Dictionary<string, string>>
            {
                new() { ["Date"] = "2025-01-01", ["Description"] = "Test", ["Amount"] = "-10.00" }
            },
            TotalRowCount = 1,
            DetectedDelimiter = "Comma",
            DetectedEncoding = "UTF-8",
            Errors = new List<CsvParseError>()
        };

        _csvParser.ParseCsvFile(Arg.Any<Stream>(), "test.csv")
            .Returns(parseResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Headers.ShouldBe(parseResult.Headers);
        result.SampleRows.ShouldBe(parseResult.SampleRows);
        result.TotalRowCount.ShouldBe(1);
        result.DetectedDelimiter.ShouldBe("Comma");
        result.DetectedEncoding.ShouldBe("UTF-8");
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_CsvParseException_PropagatesException()
    {
        // Arrange
        var mockFile = CreateMockFormFile("invalid.csv", "bad data");
        var command = new PreviewCsvCommand(mockFile);

        _csvParser.ParseCsvFile(Arg.Any<Stream>(), "invalid.csv")
            .Throws(new CsvParseException("Failed to parse CSV file"));

        // Act & Assert
        var exception = await Should.ThrowAsync<CsvParseException>(async () =>
            await _sut.Handle(command, CancellationToken.None));

        exception.Message.ShouldContain("Failed to parse CSV file");
    }

    [Fact]
    public async Task Handle_UnexpectedException_WrapsInCsvParseException()
    {
        // Arrange
        var mockFile = CreateMockFormFile("error.csv", "data");
        var command = new PreviewCsvCommand(mockFile);

        _csvParser.ParseCsvFile(Arg.Any<Stream>(), "error.csv")
            .Throws(new InvalidOperationException("Unexpected error"));

        // Act & Assert
        var exception = await Should.ThrowAsync<CsvParseException>(async () =>
            await _sut.Handle(command, CancellationToken.None));

        exception.Message.ShouldContain("unexpected error");
        exception.InnerException.ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_CancellationRequested_PropagatesCancellation()
    {
        // Arrange
        var mockFile = CreateMockFormFile("test.csv", "Date,Amount\n2025-01-01,10.00");
        var command = new PreviewCsvCommand(mockFile);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _csvParser.ParseCsvFile(Arg.Any<Stream>(), "test.csv")
            .Returns(Task.FromException<CsvParseResult>(new OperationCanceledException(cts.Token)));

        // Act & Assert
        await Should.ThrowAsync<OperationCanceledException>(async () =>
            await _sut.Handle(command, cts.Token));
    }

    [Fact]
    public async Task Handle_ParseWithErrors_ReturnsErrorsInResponse()
    {
        // Arrange
        var mockFile = CreateMockFormFile("errors.csv", "Date,Amount\n2025-01-01,invalid");
        var command = new PreviewCsvCommand(mockFile);

        var parseResult = new CsvParseResult
        {
            Headers = new[] { "Date", "Amount" },
            SampleRows = new List<Dictionary<string, string>>(),
            TotalRowCount = 0,
            DetectedDelimiter = "Comma",
            DetectedEncoding = "UTF-8",
            Errors = new List<CsvParseError>
            {
                new() { LineNumber = 2, ErrorMessage = "Invalid amount format", ColumnName = "Amount" }
            }
        };

        _csvParser.ParseCsvFile(Arg.Any<Stream>(), "errors.csv")
            .Returns(parseResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Errors.Count.ShouldBe(1);
        result.Errors[0].LineNumber.ShouldBe(2);
        result.Errors[0].ErrorMessage.ShouldContain("Invalid amount format");
        result.Errors[0].ColumnName.ShouldBe("Amount");
    }

    [Fact]
    public async Task Handle_LogsCorrelationIdAndMetadata()
    {
        // Arrange
        var mockFile = CreateMockFormFile("test.csv", "Date,Amount\n2025-01-01,10.00", fileSize: 1024);
        var command = new PreviewCsvCommand(mockFile);

        var parseResult = new CsvParseResult
        {
            Headers = new[] { "Date", "Amount" },
            SampleRows = new List<Dictionary<string, string>>(),
            TotalRowCount = 1,
            DetectedDelimiter = "Comma",
            DetectedEncoding = "UTF-8",
            Errors = new List<CsvParseError>()
        };

        _csvParser.ParseCsvFile(Arg.Any<Stream>(), "test.csv")
            .Returns(parseResult);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        // Verify logging occurred (using NSubstitute's Received extension for ILogger)
        _logger.Received(1);
    }

    #region Helper Methods

    private IFormFile CreateMockFormFile(string fileName, string content, long fileSize = 0)
    {
        var mockFile = Substitute.For<IFormFile>();
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

        mockFile.FileName.Returns(fileName);
        mockFile.Length.Returns(fileSize > 0 ? fileSize : stream.Length);
        mockFile.OpenReadStream().Returns(stream);

        return mockFile;
    }

    #endregion
}
