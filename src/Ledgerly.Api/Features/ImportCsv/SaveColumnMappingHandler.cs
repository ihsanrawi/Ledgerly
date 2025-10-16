using System.Text.Json;
using Ledgerly.Api.Common.Data;
using Ledgerly.Api.Common.Data.Entities;
using Ledgerly.Contracts.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine handler for SaveColumnMappingCommand.
/// Persists user's manual column mapping for future CSV imports.
/// Story 2.4 - Manual Column Mapping Interface (AC: 5, 6).
/// </summary>
public class SaveColumnMappingHandler
{
    private readonly LedgerlyDbContext _dbContext;
    private readonly ILogger<SaveColumnMappingHandler> _logger;

    public SaveColumnMappingHandler(
        LedgerlyDbContext dbContext,
        ILogger<SaveColumnMappingHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<SaveColumnMappingResponse> Handle(SaveColumnMappingCommand command, CancellationToken ct)
    {
        _logger.LogInformation(
            "Saving column mapping. BankIdentifier: {BankIdentifier}, ColumnCount: {ColumnCount}",
            command.BankIdentifier, command.ColumnMappings.Count);

        try
        {
            // Check if mapping already exists for this bank identifier
            var existingMapping = await _dbContext.ColumnMappingRules
                .FirstOrDefaultAsync(r => r.BankIdentifier == command.BankIdentifier && r.IsActive, ct);

            if (existingMapping != null)
            {
                // Update existing mapping
                existingMapping.BankMatchPattern = command.FileNamePattern;
                existingMapping.HeaderSignature = JsonSerializer.Serialize(command.HeaderSignature);
                existingMapping.ColumnMappings = JsonSerializer.Serialize(command.ColumnMappings);
                existingMapping.LastUsedAt = DateTime.UtcNow;

                _logger.LogInformation(
                    "Updating existing column mapping. Id: {Id}, BankIdentifier: {BankIdentifier}",
                    existingMapping.Id, existingMapping.BankIdentifier);
            }
            else
            {
                // Create new mapping
                var newMapping = new ColumnMappingRule
                {
                    Id = Guid.NewGuid(),
                    BankIdentifier = command.BankIdentifier,
                    BankMatchPattern = command.FileNamePattern,
                    HeaderSignature = JsonSerializer.Serialize(command.HeaderSignature),
                    ColumnMappings = JsonSerializer.Serialize(command.ColumnMappings),
                    CreatedAt = DateTime.UtcNow,
                    LastUsedAt = DateTime.UtcNow,
                    TimesUsed = 0,
                    IsActive = true
                };

                await _dbContext.ColumnMappingRules.AddAsync(newMapping, ct);

                _logger.LogInformation(
                    "Creating new column mapping. Id: {Id}, BankIdentifier: {BankIdentifier}",
                    newMapping.Id, newMapping.BankIdentifier);

                existingMapping = newMapping;
            }

            await _dbContext.SaveChangesAsync(ct);

            return new SaveColumnMappingResponse
            {
                Id = existingMapping.Id,
                Message = $"Mapping saved successfully for {command.BankIdentifier}"
            };
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex,
                "Concurrency conflict when saving column mapping. BankIdentifier: {BankIdentifier}",
                command.BankIdentifier);

            throw new InvalidOperationException(
                "The mapping was modified by another user. Please reload and try again.", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Unexpected error saving column mapping. BankIdentifier: {BankIdentifier}",
                command.BankIdentifier);

            throw new InvalidOperationException(
                "Failed to save column mapping. Please try again.", ex);
        }
    }
}
