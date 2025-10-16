namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine command to delete a saved column mapping rule.
/// Story 2.4 - Manual Column Mapping Interface (AC: 6).
/// </summary>
public record DeleteColumnMappingCommand
{
    /// <summary>
    /// Unique identifier of the mapping rule to delete.
    /// </summary>
    public Guid Id { get; init; }
}
