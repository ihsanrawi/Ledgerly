namespace Ledgerly.Contracts.Dtos;

/// <summary>
/// Result of automatic column detection with confidence scores.
/// </summary>
public record ColumnDetectionResult
{
    /// <summary>
    /// Detected column mappings.
    /// Key: CSV header name, Value: Field type (date, amount, description, memo, balance, debit, credit)
    /// </summary>
    public Dictionary<string, string> DetectedMappings { get; init; } = new();

    /// <summary>
    /// Confidence scores for each detected field type.
    /// Key: Field type, Value: Confidence score (0.0-1.0)
    /// </summary>
    public Dictionary<string, decimal> ConfidenceScores { get; init; } = new();

    /// <summary>
    /// User-friendly warnings for low-confidence detections.
    /// </summary>
    public List<string> Warnings { get; init; } = new();

    /// <summary>
    /// True if date and amount detected with confidence >0.7.
    /// </summary>
    public bool AllRequiredFieldsDetected { get; init; }
}
