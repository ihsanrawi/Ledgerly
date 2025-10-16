using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Ledgerly.Contracts.Dtos;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Service interface for column detection in CSV files.
/// </summary>
public interface IColumnDetectionService
{
    Task<ColumnDetectionResult> DetectColumns(string[] headers, List<Dictionary<string, string>> sampleRows);
}

/// <summary>
/// Service for automatically detecting column types in CSV files using pattern matching.
/// Uses column-name-mapping.json reference data and data validation.
/// </summary>
public class ColumnDetectionService : IColumnDetectionService
{
    private readonly ILogger<ColumnDetectionService> _logger;
    private readonly Dictionary<string, List<string>> _columnPatterns;
    private readonly Dictionary<string, Dictionary<string, decimal>> _confidenceWeights;

    // Date format patterns ordered by frequency (from CSV_FORMAT_ANALYSIS.md)
    private static readonly string[] DateFormats = new[]
    {
        "yyyy-MM-dd",      // ISO format (33% - most reliable)
        "M/d/yyyy",        // US format non-padded (common)
        "MM/dd/yyyy",      // US format padded (50%)
        "d.M.yyyy",        // European format non-padded
        "dd.MM.yyyy",      // European format padded (17%)
        "MMM d, yyyy",     // Text month format (rare)
        "d MMM yyyy",      // European text format
    };

    private static readonly Regex AmountPattern = new(@"^[\(\$€£]?-?\d{1,3}([\.,]\d{3})*[\.,]?\d{0,2}[\)]?$", RegexOptions.Compiled);
    private static readonly Regex NegativeParenthesesPattern = new(@"^\((\d+(?:[\.,]\d+)?)\)$", RegexOptions.Compiled);

    public ColumnDetectionService(ILogger<ColumnDetectionService> logger)
    {
        _logger = logger;
        _columnPatterns = LoadColumnPatterns();
        _confidenceWeights = LoadConfidenceWeights();
    }

    public async Task<ColumnDetectionResult> DetectColumns(string[] headers, List<Dictionary<string, string>> sampleRows)
    {
        _logger.LogInformation(
            "Starting column detection. HeaderCount: {HeaderCount}, SampleRowCount: {SampleRowCount}",
            headers.Length, sampleRows.Count);

        var detectedMappings = new Dictionary<string, string>();
        var confidenceScores = new Dictionary<string, decimal>();
        var warnings = new List<string>();

        try
        {
            // Detect each column type
            var dateResult = await DetectDateColumn(headers, sampleRows);
            var amountResult = await DetectAmountColumn(headers, sampleRows, dateResult.columnName);
            var descriptionResult = await DetectDescriptionColumn(headers, sampleRows, dateResult.columnName, amountResult.columnName);
            var memoResult = await DetectMemoColumn(headers, sampleRows, dateResult.columnName, amountResult.columnName, descriptionResult.columnName);

            // Add detected mappings
            AddMappingIfDetected(detectedMappings, confidenceScores, dateResult, "date");
            AddMappingIfDetected(detectedMappings, confidenceScores, amountResult, "amount");
            AddMappingIfDetected(detectedMappings, confidenceScores, descriptionResult, "description");
            AddMappingIfDetected(detectedMappings, confidenceScores, memoResult, "memo");

            // Check for split debit/credit columns (if single amount not found)
            if (string.IsNullOrEmpty(amountResult.columnName))
            {
                var debitResult = await DetectColumnByType(headers, sampleRows, "debit");
                var creditResult = await DetectColumnByType(headers, sampleRows, "credit");

                AddMappingIfDetected(detectedMappings, confidenceScores, debitResult, "debit");
                AddMappingIfDetected(detectedMappings, confidenceScores, creditResult, "credit");

                // Update amount confidence based on debit/credit detection
                if (!string.IsNullOrEmpty(debitResult.columnName) || !string.IsNullOrEmpty(creditResult.columnName))
                {
                    var avgConfidence = ((debitResult.confidence + creditResult.confidence) / 2);
                    confidenceScores["amount"] = avgConfidence;
                }
            }

            // Detect optional balance column
            var balanceResult = await DetectColumnByType(headers, sampleRows, "balance");
            AddMappingIfDetected(detectedMappings, confidenceScores, balanceResult, "balance");

            // Validate required fields and generate warnings
            var dateConfidence = confidenceScores.GetValueOrDefault("date", 0);
            var amountConfidence = confidenceScores.GetValueOrDefault("amount", 0);

            if (dateConfidence < 0.7m)
            {
                warnings.Add("Date column not detected with sufficient confidence. Please verify column mapping.");
            }

            if (amountConfidence < 0.7m)
            {
                warnings.Add("Amount column not detected with sufficient confidence. Please verify column mapping.");
            }

            var allRequiredFieldsDetected = dateConfidence >= 0.7m && amountConfidence >= 0.7m;

            _logger.LogInformation(
                "Column detection completed. DetectedColumns: {DetectedCount}, AllRequiredFieldsDetected: {AllRequired}",
                detectedMappings.Count, allRequiredFieldsDetected);

            return new ColumnDetectionResult
            {
                DetectedMappings = detectedMappings,
                ConfidenceScores = confidenceScores,
                Warnings = warnings,
                AllRequiredFieldsDetected = allRequiredFieldsDetected
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during column detection");

            // Return empty result rather than throwing
            return new ColumnDetectionResult
            {
                Warnings = new List<string> { "Column detection failed. Please map columns manually." },
                AllRequiredFieldsDetected = false
            };
        }
    }

    private async Task<(string? columnName, decimal confidence)> DetectDateColumn(string[] headers, List<Dictionary<string, string>> sampleRows)
    {
        return await Task.Run(() =>
        {
            var candidates = new List<(string header, decimal headerScore, decimal dataScore)>();

            foreach (var header in headers)
            {
                var headerScore = CalculateHeaderConfidence(header, "date");
                if (headerScore == 0) continue;

                // Validate data: check if sample rows contain valid dates
                var dataScore = ValidateDateColumn(header, sampleRows);

                candidates.Add((header, headerScore, dataScore));
            }

            if (candidates.Count == 0)
                return (null, 0);

            // Best candidate: highest combined score
            var best = candidates.OrderByDescending(c => (c.headerScore + c.dataScore) / 2).First();
            var confidence = (best.headerScore + best.dataScore) / 2;

            _logger.LogDebug("Detected date column: {Header}, Confidence: {Confidence}", best.header, confidence);
            return (best.header, confidence);
        });
    }

    private async Task<(string? columnName, decimal confidence)> DetectAmountColumn(
        string[] headers,
        List<Dictionary<string, string>> sampleRows,
        string? dateColumnName)
    {
        return await Task.Run(() =>
        {
            var candidates = new List<(string header, decimal headerScore, decimal dataScore)>();

            foreach (var header in headers)
            {
                // Skip date column
                if (header == dateColumnName) continue;

                var headerScore = CalculateHeaderConfidence(header, "amount");
                if (headerScore == 0) continue;

                // Validate data: check if sample rows contain valid amounts
                var dataScore = ValidateAmountColumn(header, sampleRows);

                candidates.Add((header, headerScore, dataScore));
            }

            if (candidates.Count == 0)
                return (null, 0);

            var best = candidates.OrderByDescending(c => (c.headerScore + c.dataScore) / 2).First();
            var confidence = (best.headerScore + best.dataScore) / 2;

            _logger.LogDebug("Detected amount column: {Header}, Confidence: {Confidence}", best.header, confidence);
            return (best.header, confidence);
        });
    }

    private async Task<(string? columnName, decimal confidence)> DetectDescriptionColumn(
        string[] headers,
        List<Dictionary<string, string>> sampleRows,
        string? dateColumnName,
        string? amountColumnName)
    {
        return await Task.Run(() =>
        {
            var candidates = new List<(string header, decimal headerScore, decimal dataScore)>();

            foreach (var header in headers)
            {
                // Skip already detected columns
                if (header == dateColumnName || header == amountColumnName) continue;

                var headerScore = CalculateHeaderConfidence(header, "description");
                if (headerScore == 0) continue;

                // Validate data: check if column contains text (not numbers/dates)
                var dataScore = ValidateTextColumn(header, sampleRows);

                candidates.Add((header, headerScore, dataScore));
            }

            // If no header match, find longest text column as fallback
            if (candidates.Count == 0)
            {
                var textColumns = headers
                    .Where(h => h != dateColumnName && h != amountColumnName)
                    .Select(h => (header: h, avgLength: CalculateAverageTextLength(h, sampleRows)))
                    .Where(c => c.avgLength > 5) // Minimum average length for description
                    .OrderByDescending(c => c.avgLength)
                    .FirstOrDefault();

                if (textColumns != default)
                {
                    _logger.LogDebug("Detected description column by fallback (longest text): {Header}", textColumns.header);
                    return (textColumns.header, 0.6m); // Lower confidence for fallback
                }

                return (null, 0);
            }

            var best = candidates.OrderByDescending(c => (c.headerScore + c.dataScore) / 2).First();
            var confidence = (best.headerScore + best.dataScore) / 2;

            _logger.LogDebug("Detected description column: {Header}, Confidence: {Confidence}", best.header, confidence);
            return (best.header, confidence);
        });
    }

    private async Task<(string? columnName, decimal confidence)> DetectMemoColumn(
        string[] headers,
        List<Dictionary<string, string>> sampleRows,
        string? dateColumnName,
        string? amountColumnName,
        string? descriptionColumnName)
    {
        return await Task.Run(() =>
        {
            var candidates = new List<(string header, decimal headerScore, decimal dataScore)>();

            foreach (var header in headers)
            {
                // Skip already detected columns
                if (header == dateColumnName || header == amountColumnName || header == descriptionColumnName)
                    continue;

                var headerScore = CalculateHeaderConfidence(header, "description"); // Use description patterns
                if (headerScore == 0) continue;

                var dataScore = ValidateTextColumn(header, sampleRows);

                candidates.Add((header, headerScore, dataScore));
            }

            if (candidates.Count == 0)
                return (null, 0);

            var best = candidates.OrderByDescending(c => (c.headerScore + c.dataScore) / 2).First();
            var confidence = (best.headerScore + best.dataScore) / 2;

            _logger.LogDebug("Detected memo column: {Header}, Confidence: {Confidence}", best.header, confidence);
            return (best.header, confidence);
        });
    }

    private async Task<(string? columnName, decimal confidence)> DetectColumnByType(
        string[] headers,
        List<Dictionary<string, string>> sampleRows,
        string fieldType)
    {
        return await Task.Run(() =>
        {
            var candidates = new List<(string header, decimal headerScore, decimal dataScore)>();

            foreach (var header in headers)
            {
                var headerScore = CalculateHeaderConfidence(header, fieldType);
                if (headerScore == 0) continue;

                decimal dataScore = fieldType switch
                {
                    "debit" or "credit" => ValidateAmountColumn(header, sampleRows),
                    "balance" => ValidateAmountColumn(header, sampleRows),
                    _ => 0
                };

                candidates.Add((header, headerScore, dataScore));
            }

            if (candidates.Count == 0)
                return (null, 0);

            var best = candidates.OrderByDescending(c => (c.headerScore + c.dataScore) / 2).First();
            var confidence = (best.headerScore + best.dataScore) / 2;

            _logger.LogDebug("Detected {FieldType} column: {Header}, Confidence: {Confidence}", fieldType, best.header, confidence);
            return (best.header, confidence);
        });
    }

    private decimal CalculateHeaderConfidence(string header, string fieldType)
    {
        if (!_columnPatterns.ContainsKey(fieldType))
            return 0;

        var patterns = _columnPatterns[fieldType];
        var weights = _confidenceWeights.GetValueOrDefault(fieldType, new Dictionary<string, decimal>());

        var normalizedHeader = header.ToLowerInvariant().Trim();

        // Exact match check
        if (patterns.Contains(normalizedHeader))
        {
            return weights.GetValueOrDefault(normalizedHeader, 1.0m);
        }

        // Substring match check
        foreach (var pattern in patterns)
        {
            if (normalizedHeader.Contains(pattern))
            {
                var weight = weights.GetValueOrDefault(pattern, 0.8m);
                return weight * 0.9m; // Slightly reduce confidence for substring match
            }
        }

        return 0;
    }

    private decimal ValidateDateColumn(string columnName, List<Dictionary<string, string>> sampleRows)
    {
        if (sampleRows.Count == 0) return 0;

        var validCount = 0;
        var totalCount = 0;

        foreach (var row in sampleRows)
        {
            if (!row.TryGetValue(columnName, out var value) || string.IsNullOrWhiteSpace(value))
                continue;

            totalCount++;

            // Try parsing with multiple date formats
            foreach (var format in DateFormats)
            {
                if (DateTime.TryParseExact(value.Trim(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                {
                    validCount++;
                    break;
                }
            }
        }

        if (totalCount == 0) return 0;

        var validationRate = (decimal)validCount / totalCount;
        return validationRate; // 1.0 = 100% valid dates, 0.9 = 90%, etc.
    }

    private decimal ValidateAmountColumn(string columnName, List<Dictionary<string, string>> sampleRows)
    {
        if (sampleRows.Count == 0) return 0;

        var validCount = 0;
        var totalCount = 0;

        foreach (var row in sampleRows)
        {
            if (!row.TryGetValue(columnName, out var value) || string.IsNullOrWhiteSpace(value))
                continue;

            totalCount++;

            // Check if value matches amount pattern
            var normalized = value.Trim().Replace("$", "").Replace("€", "").Replace("£", "");

            if (AmountPattern.IsMatch(normalized))
            {
                validCount++;
            }
        }

        if (totalCount == 0) return 0;

        var validationRate = (decimal)validCount / totalCount;
        return validationRate;
    }

    private decimal ValidateTextColumn(string columnName, List<Dictionary<string, string>> sampleRows)
    {
        if (sampleRows.Count == 0) return 0;

        var textCount = 0;
        var totalCount = 0;

        foreach (var row in sampleRows)
        {
            if (!row.TryGetValue(columnName, out var value) || string.IsNullOrWhiteSpace(value))
                continue;

            totalCount++;

            // Check if value is primarily text (not a number or date)
            var isNotNumber = !decimal.TryParse(value.Trim().Replace("$", "").Replace(",", ""), out _);
            var isNotDate = !DateTime.TryParse(value.Trim(), out _);

            if (isNotNumber && isNotDate && value.Trim().Length > 2)
            {
                textCount++;
            }
        }

        if (totalCount == 0) return 0;

        var validationRate = (decimal)textCount / totalCount;
        return validationRate;
    }

    private decimal CalculateAverageTextLength(string columnName, List<Dictionary<string, string>> sampleRows)
    {
        if (sampleRows.Count == 0) return 0;

        var totalLength = 0;
        var count = 0;

        foreach (var row in sampleRows)
        {
            if (row.TryGetValue(columnName, out var value) && !string.IsNullOrWhiteSpace(value))
            {
                totalLength += value.Trim().Length;
                count++;
            }
        }

        return count > 0 ? (decimal)totalLength / count : 0;
    }

    private void AddMappingIfDetected(
        Dictionary<string, string> mappings,
        Dictionary<string, decimal> scores,
        (string? columnName, decimal confidence) result,
        string fieldType)
    {
        if (!string.IsNullOrEmpty(result.columnName) && result.confidence > 0)
        {
            mappings[result.columnName] = fieldType;
            scores[fieldType] = result.confidence;
        }
    }

    private Dictionary<string, List<string>> LoadColumnPatterns()
    {
        // Hardcoded from column-name-mapping.json to avoid file I/O dependency
        return new Dictionary<string, List<string>>
        {
            ["date"] = new List<string>
            {
                "date", "transaction date", "post date", "posting date", "effective date",
                "value date", "datum", "fecha", "trans date", "tran date"
            },
            ["description"] = new List<string>
            {
                "description", "payee", "merchant", "memo", "details", "transaction description",
                "beschreibung", "descripción", "vendor", "name", "trans description"
            },
            ["amount"] = new List<string>
            {
                "amount", "transaction amount", "betrag", "monto", "value", "amt"
            },
            ["debit"] = new List<string>
            {
                "debit", "withdrawal", "outflow", "payment", "withdrawals", "paid out", "expense"
            },
            ["credit"] = new List<string>
            {
                "credit", "deposit", "inflow", "receipt", "deposits", "paid in", "income"
            },
            ["balance"] = new List<string>
            {
                "balance", "running balance", "running bal", "running bal.", "available balance",
                "current balance", "saldo", "bal", "ending balance"
            }
        };
    }

    private Dictionary<string, Dictionary<string, decimal>> LoadConfidenceWeights()
    {
        // Hardcoded from column-name-mapping.json confidence values
        return new Dictionary<string, Dictionary<string, decimal>>
        {
            ["date"] = new Dictionary<string, decimal>
            {
                ["date"] = 1.0m,
                ["transaction date"] = 0.95m,
                ["post date"] = 0.9m,
                ["datum"] = 0.85m
            },
            ["description"] = new Dictionary<string, decimal>
            {
                ["description"] = 1.0m,
                ["payee"] = 0.95m,
                ["merchant"] = 0.9m,
                ["memo"] = 0.85m
            },
            ["amount"] = new Dictionary<string, decimal>
            {
                ["amount"] = 1.0m,
                ["transaction amount"] = 0.95m,
                ["betrag"] = 0.9m
            },
            ["debit"] = new Dictionary<string, decimal>
            {
                ["debit"] = 1.0m,
                ["withdrawal"] = 0.95m,
                ["outflow"] = 0.9m
            },
            ["credit"] = new Dictionary<string, decimal>
            {
                ["credit"] = 1.0m,
                ["deposit"] = 0.95m,
                ["inflow"] = 0.9m
            },
            ["balance"] = new Dictionary<string, decimal>
            {
                ["balance"] = 1.0m,
                ["running balance"] = 0.95m,
                ["saldo"] = 0.9m
            }
        };
    }
}
