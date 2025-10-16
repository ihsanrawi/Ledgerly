using System.Text.Json;
using Ledgerly.Api.Common.Data;
using Ledgerly.Contracts.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine handler for PreviewCsvCommand.
/// Orchestrates CSV file upload, validation, parsing, and preview generation.
/// </summary>
public class PreviewCsvHandler
{
    private readonly ICsvParserService _csvParser;
    private readonly IColumnDetectionService _columnDetection;
    private readonly LedgerlyDbContext _dbContext;
    private readonly ILogger<PreviewCsvHandler> _logger;
    private const int TimeoutSeconds = 30;

    public PreviewCsvHandler(
        ICsvParserService csvParser,
        IColumnDetectionService columnDetection,
        LedgerlyDbContext dbContext,
        ILogger<PreviewCsvHandler> logger)
    {
        _csvParser = csvParser;
        _columnDetection = columnDetection;
        _dbContext = dbContext;
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

            // Story 2.4: Check for saved mapping BEFORE auto-detection (TECH-001 mitigation)
            Dictionary<string, string>? savedMapping = null;
            if (result.Headers.Length > 0)
            {
                savedMapping = await FindSavedMappingByHeaderSignature(result.Headers, linkedCts.Token);
            }

            // Detect columns (use saved mapping if available, otherwise auto-detect)
            ColumnDetectionResult? columnDetection = null;
            if (result.Headers.Length > 0 && result.SampleRows.Count > 0)
            {
                if (savedMapping != null)
                {
                    _logger.LogInformation("Applying saved mapping to CSV preview");
                    columnDetection = CreateDetectionResultFromSavedMapping(savedMapping);
                }
                else
                {
                    columnDetection = await _columnDetection.DetectColumns(result.Headers, result.SampleRows);
                }
            }

            // Story 2.4: Determine if manual mapping is required (DATA-001 mitigation)
            var requiresManualMapping = columnDetection != null && !columnDetection.AllRequiredFieldsDetected;

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "CSV preview completed. Duration: {Duration}ms, Rows: {RowCount}, Errors: {ErrorCount}, ColumnsDetected: {ColumnsDetected}, RequiresManualMapping: {RequiresManualMapping}",
                duration.TotalMilliseconds, result.TotalRowCount, result.Errors.Count, columnDetection?.DetectedMappings.Count ?? 0, requiresManualMapping);

            // Map to response DTO
            return new PreviewCsvResponse
            {
                Headers = result.Headers,
                SampleRows = result.SampleRows,
                TotalRowCount = result.TotalRowCount,
                DetectedDelimiter = result.DetectedDelimiter,
                DetectedEncoding = result.DetectedEncoding,
                Errors = result.Errors,
                ColumnDetection = columnDetection,
                RequiresManualMapping = requiresManualMapping,
                SavedMapping = savedMapping,
                AvailableHeaders = result.Headers
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

    /// <summary>
    /// Find saved mapping by exact header signature match (TECH-001 mitigation).
    /// Story 2.4 - Manual Column Mapping Interface.
    /// </summary>
    private async Task<Dictionary<string, string>?> FindSavedMappingByHeaderSignature(
        string[] headers,
        CancellationToken ct)
    {
        try
        {
            var headerSignatureJson = JsonSerializer.Serialize(headers);

            var mapping = await _dbContext.ColumnMappingRules
                .Where(r => r.IsActive && r.HeaderSignature == headerSignatureJson)
                .OrderByDescending(r => r.LastUsedAt)
                .FirstOrDefaultAsync(ct);

            if (mapping != null)
            {
                // Update last used timestamp and usage counter
                mapping.LastUsedAt = DateTime.UtcNow;
                mapping.TimesUsed++;
                await _dbContext.SaveChangesAsync(ct);

                var columnMappings = JsonSerializer.Deserialize<Dictionary<string, string>>(mapping.ColumnMappings);

                _logger.LogInformation(
                    "Found saved mapping for bank: {BankIdentifier}, TimesUsed: {TimesUsed}",
                    mapping.BankIdentifier, mapping.TimesUsed);

                return columnMappings;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error finding saved mapping by header signature");
            return null; // Gracefully degrade to auto-detection
        }
    }

    /// <summary>
    /// Create ColumnDetectionResult from saved mapping with 100% confidence.
    /// Story 2.4 - Manual Column Mapping Interface.
    /// </summary>
    private ColumnDetectionResult CreateDetectionResultFromSavedMapping(Dictionary<string, string> savedMapping)
    {
        var detectedMappings = new Dictionary<string, string>();
        var confidenceScores = new Dictionary<string, decimal>();

        foreach (var mapping in savedMapping)
        {
            detectedMappings[mapping.Key] = mapping.Value;
            confidenceScores[mapping.Value] = 1.0m; // 100% confidence for saved mappings
        }

        // Check if all required fields are present
        var hasDate = confidenceScores.ContainsKey("date");
        var hasAmount = confidenceScores.ContainsKey("amount") ||
                       confidenceScores.ContainsKey("debit") ||
                       confidenceScores.ContainsKey("credit");

        return new ColumnDetectionResult
        {
            DetectedMappings = detectedMappings,
            ConfidenceScores = confidenceScores,
            Warnings = new List<string>(),
            AllRequiredFieldsDetected = hasDate && hasAmount
        };
    }
}
