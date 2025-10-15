namespace Ledgerly.Contracts.Dtos;

/// <summary>
/// DTO representing a CSV parsing error with line number and details.
/// </summary>
public record CsvParseError
{
    public int LineNumber { get; init; }
    public string ErrorMessage { get; init; } = string.Empty;
    public string? ColumnName { get; init; }
}
