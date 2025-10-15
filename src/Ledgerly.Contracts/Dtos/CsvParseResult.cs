namespace Ledgerly.Contracts.Dtos;

/// <summary>
/// Internal result DTO from CSV parsing service.
/// Maps to PreviewCsvResponse for API response.
/// </summary>
public record CsvParseResult
{
    public string[] Headers { get; init; } = Array.Empty<string>();
    public List<Dictionary<string, string>> SampleRows { get; init; } = new();
    public int TotalRowCount { get; init; }
    public string DetectedDelimiter { get; init; } = ",";
    public string DetectedEncoding { get; init; } = "UTF-8";
    public List<CsvParseError> Errors { get; init; } = new();
}
