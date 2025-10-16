using System.Text.Json;
using Ledgerly.Api.Common.Data;
using Ledgerly.Contracts.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine handler for GetSavedMappingsQuery.
/// Retrieves all active column mapping rules sorted by last used date.
/// Story 2.4 - Manual Column Mapping Interface (AC: 6).
/// </summary>
public class GetSavedMappingsHandler
{
    private readonly LedgerlyDbContext _dbContext;
    private readonly ILogger<GetSavedMappingsHandler> _logger;

    public GetSavedMappingsHandler(
        LedgerlyDbContext dbContext,
        ILogger<GetSavedMappingsHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<List<SavedMappingDto>> Handle(GetSavedMappingsQuery query, CancellationToken ct)
    {
        _logger.LogInformation("Retrieving all saved column mappings");

        try
        {
            var rawMappings = await _dbContext.ColumnMappingRules
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.LastUsedAt)
                .ToListAsync(ct);

            var mappings = rawMappings.Select(r => new SavedMappingDto
            {
                Id = r.Id,
                BankIdentifier = r.BankIdentifier,
                ColumnMappings = JsonSerializer.Deserialize<Dictionary<string, string>>(r.ColumnMappings) ?? new Dictionary<string, string>(),
                CreatedAt = r.CreatedAt,
                LastUsedAt = r.LastUsedAt,
                TimesUsed = r.TimesUsed
            })
            .ToList();

            _logger.LogInformation("Retrieved {Count} saved column mappings", mappings.Count);

            return mappings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving saved column mappings");
            throw new InvalidOperationException("Failed to retrieve saved mappings. Please try again.", ex);
        }
    }
}
