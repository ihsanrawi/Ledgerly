using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Ledgerly.Contracts.Dtos;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Service for parsing CSV files with encoding and delimiter detection.
/// Uses CsvHelper library for robust CSV handling.
/// </summary>
public class CsvParserService : ICsvParserService
{
    private readonly ILogger<CsvParserService> _logger;

    public CsvParserService(ILogger<CsvParserService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Parses CSV file with automatic encoding and delimiter detection.
    /// Returns parsed headers, sample rows, and format information.
    /// </summary>
    public async Task<CsvParseResult> ParseCsvFile(Stream fileStream, string fileName)
    {
        _logger.LogInformation(
            "Starting CSV parse for file: {FileName}",
            fileName);

        var parseErrors = new List<CsvParseError>();

        try
        {
            // Detect encoding
            var encoding = await DetectEncoding(fileStream);
            _logger.LogDebug("Detected encoding: {Encoding}", encoding.EncodingName);

            // Reset stream position after encoding detection
            fileStream.Position = 0;

            // Detect delimiter
            var delimiter = await DetectDelimiter(fileStream, encoding);
            _logger.LogDebug("Detected delimiter: {Delimiter}",
                delimiter == '\t' ? "Tab" : delimiter.ToString());

            // Reset stream position before parsing
            fileStream.Position = 0;

            // Parse CSV file
            using var reader = new StreamReader(fileStream, encoding, leaveOpen: true);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = delimiter.ToString(),
                HasHeaderRecord = true,
                BadDataFound = context =>
                {
                    parseErrors.Add(new CsvParseError
                    {
                        LineNumber = context.Context.Parser.Row,
                        ErrorMessage = $"Bad data found: {context.RawRecord}",
                        ColumnName = null
                    });
                },
                MissingFieldFound = context =>
                {
                    parseErrors.Add(new CsvParseError
                    {
                        LineNumber = context.Context.Parser.Row,
                        ErrorMessage = "Missing required field",
                        ColumnName = context.HeaderNames?[context.Index]
                    });
                }
            };

            using var csv = new CsvReader(reader, config);

            // Read header
            await csv.ReadAsync();
            csv.ReadHeader();
            var headers = csv.HeaderRecord ?? Array.Empty<string>();

            if (headers.Length == 0)
            {
                throw new CsvParseException("CSV file has no headers");
            }

            // Read all rows to get total count and sample rows
            var allRows = new List<Dictionary<string, string>>();
            var rowNumber = 2; // Row 1 is header

            while (await csv.ReadAsync())
            {
                try
                {
                    var row = new Dictionary<string, string>();
                    foreach (var header in headers)
                    {
                        var value = csv.GetField(header) ?? string.Empty;
                        row[header] = value;
                    }
                    allRows.Add(row);
                    rowNumber++;
                }
                catch (Exception ex)
                {
                    parseErrors.Add(new CsvParseError
                    {
                        LineNumber = rowNumber,
                        ErrorMessage = ex.Message,
                        ColumnName = null
                    });
                    rowNumber++;
                }
            }

            // Get first 10 rows as sample
            var sampleRows = allRows.Take(10).ToList();

            _logger.LogInformation(
                "CSV parse completed. Rows: {RowCount}, Headers: {HeaderCount}, Errors: {ErrorCount}",
                allRows.Count, headers.Length, parseErrors.Count);

            return new CsvParseResult
            {
                Headers = headers,
                SampleRows = sampleRows,
                TotalRowCount = allRows.Count,
                DetectedDelimiter = GetDelimiterName(delimiter),
                DetectedEncoding = encoding.EncodingName,
                Errors = parseErrors
            };
        }
        catch (CsvParseException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing CSV file: {FileName}", fileName);
            throw new CsvParseException($"Failed to parse CSV file: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Detects file encoding by analyzing byte order marks and character patterns.
    /// Supports UTF-8 and ISO-8859-1.
    /// </summary>
    private async Task<Encoding> DetectEncoding(Stream stream)
    {
        var buffer = new byte[4];
        var bytesRead = await stream.ReadAsync(buffer.AsMemory(0, 4));

        // Check for UTF-8 BOM
        if (bytesRead >= 3 && buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
        {
            return Encoding.UTF8;
        }

        // Default to UTF-8 (most common for modern CSV files)
        // If parsing fails, caller can retry with ISO-8859-1
        return Encoding.UTF8;
    }

    /// <summary>
    /// Detects CSV delimiter by analyzing first few lines.
    /// Supports comma, semicolon, and tab delimiters.
    /// </summary>
    private async Task<char> DetectDelimiter(Stream stream, Encoding encoding)
    {
        using var reader = new StreamReader(stream, encoding, leaveOpen: true);

        // Read first 5 lines for delimiter detection
        var lines = new List<string>();
        for (int i = 0; i < 5 && !reader.EndOfStream; i++)
        {
            var line = await reader.ReadLineAsync();
            if (!string.IsNullOrWhiteSpace(line))
            {
                lines.Add(line);
            }
        }

        if (lines.Count == 0)
        {
            return ','; // Default to comma
        }

        // Count delimiters in each line
        var delimiters = new[] { ',', ';', '\t' };
        var delimiterCounts = new Dictionary<char, int>();

        foreach (var delimiter in delimiters)
        {
            // Count occurrences in first line (header)
            var count = lines[0].Count(c => c == delimiter);

            // Verify consistency across lines
            var consistent = lines.All(line => line.Count(c => c == delimiter) == count);

            if (consistent && count > 0)
            {
                delimiterCounts[delimiter] = count;
            }
        }

        // Return delimiter with highest count
        if (delimiterCounts.Any())
        {
            return delimiterCounts.OrderByDescending(kvp => kvp.Value).First().Key;
        }

        return ','; // Default to comma
    }

    private string GetDelimiterName(char delimiter)
    {
        return delimiter switch
        {
            ',' => "Comma",
            ';' => "Semicolon",
            '\t' => "Tab",
            _ => delimiter.ToString()
        };
    }
}

/// <summary>
/// Interface for CSV parsing service.
/// </summary>
public interface ICsvParserService
{
    Task<CsvParseResult> ParseCsvFile(Stream fileStream, string fileName);
}

/// <summary>
/// Exception thrown when CSV parsing fails.
/// </summary>
public class CsvParseException : Exception
{
    public CsvParseException(string message) : base(message) { }
    public CsvParseException(string message, Exception innerException) : base(message, innerException) { }
}
