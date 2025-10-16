using Ledgerly.Api.Common.Data;
using Microsoft.EntityFrameworkCore;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine handler for DeleteColumnMappingCommand.
/// Soft-deletes a column mapping rule by setting IsActive = false.
/// Story 2.4 - Manual Column Mapping Interface (AC: 6).
/// </summary>
public class DeleteColumnMappingHandler
{
    private readonly LedgerlyDbContext _dbContext;
    private readonly ILogger<DeleteColumnMappingHandler> _logger;

    public DeleteColumnMappingHandler(
        LedgerlyDbContext dbContext,
        ILogger<DeleteColumnMappingHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Handle(DeleteColumnMappingCommand command, CancellationToken ct)
    {
        _logger.LogInformation("Deleting column mapping. Id: {Id}", command.Id);

        try
        {
            var mapping = await _dbContext.ColumnMappingRules
                .FirstOrDefaultAsync(r => r.Id == command.Id && r.IsActive, ct);

            if (mapping == null)
            {
                _logger.LogWarning("Column mapping not found. Id: {Id}", command.Id);
                throw new InvalidOperationException($"Column mapping with ID {command.Id} not found or already deleted.");
            }

            // Soft delete
            mapping.IsActive = false;

            await _dbContext.SaveChangesAsync(ct);

            _logger.LogInformation(
                "Column mapping deleted successfully. Id: {Id}, BankIdentifier: {BankIdentifier}",
                mapping.Id, mapping.BankIdentifier);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict when deleting column mapping. Id: {Id}", command.Id);
            throw new InvalidOperationException("The mapping was modified by another user. Please reload and try again.", ex);
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Unexpected error deleting column mapping. Id: {Id}", command.Id);
            throw new InvalidOperationException("Failed to delete column mapping. Please try again.", ex);
        }
    }
}
