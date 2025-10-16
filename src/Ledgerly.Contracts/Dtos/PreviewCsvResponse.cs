namespace Ledgerly.Contracts.Dtos;

/// <summary>
/// Response DTO containing CSV preview data with format detection.
/// </summary>
public record PreviewCsvResponse
{
    public string[] Headers { get; init; } = Array.Empty<string>();
    public List<Dictionary<string, string>> SampleRows { get; init; } = new();
    public int TotalRowCount { get; init; }
    public string DetectedDelimiter { get; init; } = ",";
    public string DetectedEncoding { get; init; } = "UTF-8";
    public List<CsvParseError> Errors { get; init; } = new();

    /// <summary>
    /// Column detection results with confidence scores.
    /// Added in Story 2.3 for automatic column detection.
    /// </summary>
    public ColumnDetectionResult? ColumnDetection { get; init; }
}
