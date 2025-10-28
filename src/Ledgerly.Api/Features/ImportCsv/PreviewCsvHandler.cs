using System.Text.Json;
using Ledgerly.Api.Common.Data;
using Ledgerly.Contracts.Dtos;
using Microsoft.EntityFrameworkCore;
using Wolverine;

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
    private readonly IMessageBus _messageBus;
    private readonly ILogger<PreviewCsvHandler> _logger;
    private const int TimeoutSeconds = 30;

    public PreviewCsvHandler(
        ICsvParserService csvParser,
        IColumnDetectionService columnDetection,
        LedgerlyDbContext dbContext,
        IMessageBus messageBus,
        ILogger<PreviewCsvHandler> logger)
    {
        _csvParser = csvParser;
        _columnDetection = columnDetection;
        _dbContext = dbContext;
        _messageBus = messageBus;
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

            // Story 2.5: Detect duplicates and suggest categories if column mapping is complete
            var duplicates = new List<DuplicateTransactionDto>();
            var suggestions = new List<CategorySuggestionDto>();

            if (columnDetection != null && columnDetection.AllRequiredFieldsDetected)
            {
                try
                {
                    // Parse CSV rows into ParsedTransactionDto format
                    var parsedTransactions = ParseTransactionsFromCsv(
                        result.SampleRows,
                        columnDetection.DetectedMappings);

                    if (parsedTransactions.Count > 0)
                    {
                        // Detect duplicates
                        var duplicateQuery = new DetectDuplicatesQuery
                        {
                            Transactions = parsedTransactions
                        };
                        var duplicateResponse = await _messageBus.InvokeAsync<DetectDuplicatesResponse>(
                            duplicateQuery, linkedCts.Token);
                        duplicates = duplicateResponse.Duplicates;

                        _logger.LogInformation(
                            "Duplicate detection found {DuplicateCount} duplicates out of {TotalCount} transactions",
                            duplicates.Count, parsedTransactions.Count);

                        // Get category suggestions
                        var suggestionQuery = new GetCategorySuggestionsQuery
                        {
                            Transactions = parsedTransactions
                        };
                        var suggestionResponse = await _messageBus.InvokeAsync<GetCategorySuggestionsResponse>(
                            suggestionQuery, linkedCts.Token);
                        suggestions = suggestionResponse.Suggestions;

                        _logger.LogInformation(
                            "Category suggestion found {SuggestionCount} suggestions out of {TotalCount} transactions",
                            suggestions.Count, parsedTransactions.Count);
                    }
                }
                catch (Exception ex)
                {
                    // Log error but don't fail preview - duplicate detection is optional enhancement
                    _logger.LogWarning(ex,
                        "Error during duplicate detection or category suggestion - continuing with preview");
                }
            }

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "CSV preview completed. Duration: {Duration}ms, Rows: {RowCount}, Errors: {ErrorCount}, ColumnsDetected: {ColumnsDetected}, RequiresManualMapping: {RequiresManualMapping}, Duplicates: {DuplicateCount}, Suggestions: {SuggestionCount}",
                duration.TotalMilliseconds, result.TotalRowCount, result.Errors.Count, columnDetection?.DetectedMappings.Count ?? 0, requiresManualMapping, duplicates.Count, suggestions.Count);

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
                AvailableHeaders = result.Headers,
                Duplicates = duplicates,
                Suggestions = suggestions
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

    /// <summary>
    /// Parse CSV rows into ParsedTransactionDto format for duplicate detection and category suggestions.
    /// Story 2.5 - Duplicate Detection and Category Suggestions integration.
    /// </summary>
    private List<ParsedTransactionDto> ParseTransactionsFromCsv(
        List<Dictionary<string, string>> csvRows,
        Dictionary<string, string> columnMappings)
    {
        var transactions = new List<ParsedTransactionDto>();

        // Find the header names for required fields from detected mappings
        var dateHeader = columnMappings.FirstOrDefault(m => m.Value == "date").Key;
        var payeeHeader = columnMappings.FirstOrDefault(m => m.Value == "payee").Key;
        var descriptionHeader = columnMappings.FirstOrDefault(m => m.Value == "description").Key;
        var amountHeader = columnMappings.FirstOrDefault(m => m.Value == "amount").Key;
        var debitHeader = columnMappings.FirstOrDefault(m => m.Value == "debit").Key;
        var creditHeader = columnMappings.FirstOrDefault(m => m.Value == "credit").Key;

        _logger.LogInformation(
            "Column mappings: date={DateHeader}, payee={PayeeHeader}, description={DescriptionHeader}, amount={AmountHeader}",
            dateHeader, payeeHeader ?? "(null)", descriptionHeader ?? "(null)", amountHeader);

        if (string.IsNullOrEmpty(dateHeader))
        {
            _logger.LogWarning("Date header not found in column mappings - skipping transaction parsing");
            return transactions;
        }

        for (int i = 0; i < csvRows.Count; i++)
        {
            var row = csvRows[i];

            try
            {
                // Extract date
                if (!row.TryGetValue(dateHeader, out var dateStr) || string.IsNullOrWhiteSpace(dateStr))
                    continue;

                if (!DateTime.TryParse(dateStr, out var date))
                {
                    _logger.LogDebug("Failed to parse date '{DateStr}' at row {RowIndex}", dateStr, i);
                    continue;
                }

                // Extract payee (try payee first, then description as fallback)
                var payee = string.Empty;
                if (!string.IsNullOrEmpty(payeeHeader) && row.TryGetValue(payeeHeader, out var payeeValue))
                {
                    payee = payeeValue ?? string.Empty;
                }
                else if (!string.IsNullOrEmpty(descriptionHeader) && row.TryGetValue(descriptionHeader, out var descValue))
                {
                    payee = descValue ?? string.Empty;
                }

                // Extract amount (handle both single amount column and debit/credit columns)
                decimal amount = 0;
                if (!string.IsNullOrEmpty(amountHeader) && row.TryGetValue(amountHeader, out var amountStr))
                {
                    // Single amount column
                    if (decimal.TryParse(amountStr, out var parsedAmount))
                    {
                        amount = parsedAmount;
                    }
                }
                else if (!string.IsNullOrEmpty(debitHeader) && !string.IsNullOrEmpty(creditHeader))
                {
                    // Handle debit/credit columns
                    decimal debit = 0;
                    decimal credit = 0;

                    var hasDebit = row.TryGetValue(debitHeader, out var debitStr) &&
                                  !string.IsNullOrWhiteSpace(debitStr) &&
                                  decimal.TryParse(debitStr, out debit);

                    var hasCredit = row.TryGetValue(creditHeader, out var creditStr) &&
                                   !string.IsNullOrWhiteSpace(creditStr) &&
                                   decimal.TryParse(creditStr, out credit);

                    if (hasDebit)
                        amount = -Math.Abs(debit); // Debits are negative
                    else if (hasCredit)
                        amount = Math.Abs(credit); // Credits are positive
                }

                if (amount == 0)
                {
                    _logger.LogDebug("Amount is zero or failed to parse at row {RowIndex}", i);
                    continue;
                }

                transactions.Add(new ParsedTransactionDto
                {
                    Date = date,
                    Payee = payee,
                    Amount = amount,
                    RowIndex = i
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error parsing transaction at row {RowIndex}", i);
            }
        }

        _logger.LogInformation(
            "Parsed {TransactionCount} transactions from {RowCount} CSV rows for duplicate detection",
            transactions.Count, csvRows.Count);

        return transactions;
    }
}
