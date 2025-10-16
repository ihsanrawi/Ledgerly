namespace Ledgerly.Contracts.Dtos;

/// <summary>
/// Response DTO for SaveColumnMappingCommand.
/// Story 2.4 - Manual Column Mapping Interface (AC: 5).
/// </summary>
public record SaveColumnMappingResponse
{
    /// <summary>
    /// Unique identifier of the saved/updated mapping rule.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Success message to display to user.
    /// </summary>
    public string Message { get; init; } = string.Empty;
}
