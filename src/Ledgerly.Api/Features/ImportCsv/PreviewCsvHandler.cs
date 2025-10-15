using Ledgerly.Contracts.Dtos;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine handler for PreviewCsvCommand.
/// Orchestrates CSV file upload, validation, parsing, and preview generation.
/// </summary>
public class PreviewCsvHandler
{
    private readonly ICsvParserService _csvParser;
    private readonly ILogger<PreviewCsvHandler> _logger;
    private const int TimeoutSeconds = 30;

    public PreviewCsvHandler(
        ICsvParserService csvParser,
        ILogger<PreviewCsvHandler> logger)
    {
        _csvParser = csvParser;
        _logger = logger;
    }

    public async Task<PreviewCsvResponse> Handle(PreviewCsvCommand command, CancellationToken ct)
    {
        var correlationId = Guid.NewGuid();
        var startTime = DateTime.UtcNow;

        _logger.LogInformation(
            "Processing PreviewCsvCommand. CorrelationId: {CorrelationId}, FileName: {FileName}, FileSize: {FileSize}",
            correlationId, command.File.FileName, command.File.Length);

        try
        {
            // Create timeout cancellation token
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

            // Parse CSV file
            await using var stream = command.File.OpenReadStream();
            var result = await _csvParser.ParseCsvFile(stream, command.File.FileName);

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "CSV preview completed. CorrelationId: {CorrelationId}, Duration: {Duration}ms, Rows: {RowCount}, Errors: {ErrorCount}",
                correlationId, duration.TotalMilliseconds, result.TotalRowCount, result.Errors.Count);

            // Map to response DTO
            return new PreviewCsvResponse
            {
                Headers = result.Headers,
                SampleRows = result.SampleRows,
                TotalRowCount = result.TotalRowCount,
                DetectedDelimiter = result.DetectedDelimiter,
                DetectedEncoding = result.DetectedEncoding,
                Errors = result.Errors
            };
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogWarning("CSV preview cancelled by user. CorrelationId: {CorrelationId}", correlationId);
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "CSV preview timed out after {Timeout} seconds. CorrelationId: {CorrelationId}",
                TimeoutSeconds, correlationId);
            throw new CsvParseException($"CSV parsing timed out after {TimeoutSeconds} seconds. Please try a smaller file.");
        }
        catch (CsvParseException ex)
        {
            _logger.LogError(ex,
                "CSV parse error. CorrelationId: {CorrelationId}, FileName: {FileName}",
                correlationId, command.File.FileName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error during CSV preview. CorrelationId: {CorrelationId}, FileName: {FileName}",
                correlationId, command.File.FileName);
            throw new CsvParseException("An unexpected error occurred while parsing the CSV file.", ex);
        }
    }
}
