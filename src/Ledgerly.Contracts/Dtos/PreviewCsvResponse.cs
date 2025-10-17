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

    /// <summary>
    /// Flag indicating manual mapping is required.
    /// True if auto-detection confidence < 0.7 for required fields (date, amount).
    /// Added in Story 2.4 for manual column mapping.
    /// </summary>
    public bool RequiresManualMapping { get; init; }

    /// <summary>
    /// Pre-loaded saved mapping if bank match found.
    /// Used to pre-populate manual mapping UI.
    /// Added in Story 2.4 for manual column mapping.
    /// </summary>
    public Dictionary<string, string>? SavedMapping { get; init; }

    /// <summary>
    /// All CSV headers available for drag-drop manual mapping.
    /// Added in Story 2.4 for manual column mapping.
    /// </summary>
    public string[] AvailableHeaders { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Matched existing transactions flagged as duplicates.
    /// Added in Story 2.5 for duplicate detection.
    /// </summary>
    public List<DuplicateTransactionDto> Duplicates { get; init; } = new();

    /// <summary>
    /// Category suggestions for each transaction based on ImportRules.
    /// Added in Story 2.5 for category suggestion.
    /// </summary>
    public List<CategorySuggestionDto> Suggestions { get; init; } = new();
}
