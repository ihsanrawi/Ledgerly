using Ledgerly.Contracts.Dtos;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine handler for PreviewCsvCommand.
/// Orchestrates CSV file upload, validation, parsing, and preview generation.
/// </summary>
public class PreviewCsvHandler
{
    private readonly ICsvParserService _csvParser;
    private readonly IColumnDetectionService _columnDetection;
    private readonly ILogger<PreviewCsvHandler> _logger;
    private const int TimeoutSeconds = 30;

    public PreviewCsvHandler(
        ICsvParserService csvParser,
        IColumnDetectionService columnDetection,
        ILogger<PreviewCsvHandler> logger)
    {
        _csvParser = csvParser;
        _columnDetection = columnDetection;
        _logger = logger;
    }

    public async Task<PreviewCsvResponse> Handle(PreviewCsvCommand command, CancellationToken ct)
    {
        var startTime = DateTime.UtcNow;

        _logger.LogInformation(
            "Processing PreviewCsvCommand. FileName: {FileName}, FileSize: {FileSize}",
            command.File.FileName, command.File.Length);

        try
        {
            // Create timeout cancellation token
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutCts.Token);

            // Parse CSV file
            await using var stream = command.File.OpenReadStream();
            var result = await _csvParser.ParseCsvFile(stream, command.File.FileName);

            // Detect columns
            ColumnDetectionResult? columnDetection = null;
            if (result.Headers.Length > 0 && result.SampleRows.Count > 0)
            {
                columnDetection = await _columnDetection.DetectColumns(result.Headers, result.SampleRows);
            }

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "CSV preview completed. Duration: {Duration}ms, Rows: {RowCount}, Errors: {ErrorCount}, ColumnsDetected: {ColumnsDetected}",
                duration.TotalMilliseconds, result.TotalRowCount, result.Errors.Count, columnDetection?.DetectedMappings.Count ?? 0);

            // Map to response DTO
            return new PreviewCsvResponse
            {
                Headers = result.Headers,
                SampleRows = result.SampleRows,
                TotalRowCount = result.TotalRowCount,
                DetectedDelimiter = result.DetectedDelimiter,
                DetectedEncoding = result.DetectedEncoding,
                Errors = result.Errors,
                ColumnDetection = columnDetection
            };
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogWarning("CSV preview cancelled by user");
            throw;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning(
                "CSV preview timed out after {Timeout} seconds",
                TimeoutSeconds);
            throw new CsvParseException($"CSV parsing timed out after {TimeoutSeconds} seconds. Please try a smaller file.");
        }
        catch (CsvParseException ex)
        {
            _logger.LogError(ex,
                "CSV parse error. FileName: {FileName}",
                command.File.FileName);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error during CSV preview. FileName: {FileName}",
                command.File.FileName);
            throw new CsvParseException("An unexpected error occurred while parsing the CSV file.", ex);
        }
    }
}
