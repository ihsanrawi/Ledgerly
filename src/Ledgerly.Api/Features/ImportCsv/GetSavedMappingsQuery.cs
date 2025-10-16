namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine query to retrieve all saved column mapping rules.
/// Story 2.4 - Manual Column Mapping Interface (AC: 6).
/// </summary>
public record GetSavedMappingsQuery
{
    // Empty query - returns all active mappings for user
    // Future enhancement: Add filtering by bank identifier or date range
}
