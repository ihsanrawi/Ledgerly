using System.Text.Json;
using Ledgerly.Api.Common.Data;
using Ledgerly.Api.Common.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ledgerly.Api.Features.ImportCsv.Tests;

/// <summary>
/// Unit tests for GetSavedMappingsHandler.
/// Story 2.4 - Manual Column Mapping Interface (AC: 6).
/// </summary>
public class GetSavedMappingsHandlerTests : IDisposable
{
    private readonly LedgerlyDbContext _dbContext;
    private readonly ILogger<GetSavedMappingsHandler> _logger;
    private readonly GetSavedMappingsHandler _handler;

    public GetSavedMappingsHandlerTests()
    {
        var options = new DbContextOptionsBuilder<LedgerlyDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;
        _dbContext = new LedgerlyDbContext(options);

        _logger = Substitute.For<ILogger<GetSavedMappingsHandler>>();
        _handler = new GetSavedMappingsHandler(_dbContext, _logger);
    }

    [Fact]
    public async Task Handle_NoMappings_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetSavedMappingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_WithMappings_ShouldReturnAll()
    {
        // Arrange
        await _dbContext.ColumnMappingRules.AddRangeAsync(
            new ColumnMappingRule
            {
                Id = Guid.NewGuid(),
                BankIdentifier = "Chase Checking",
                HeaderSignature = JsonSerializer.Serialize(new[] { "Date", "Amount" }),
                ColumnMappings = JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    { "Date", "date" },
                    { "Amount", "amount" }
                }),
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                LastUsedAt = DateTime.UtcNow.AddDays(-1),
                TimesUsed = 3,
                IsActive = true
            },
            new ColumnMappingRule
            {
                Id = Guid.NewGuid(),
                BankIdentifier = "Bank of America",
                HeaderSignature = JsonSerializer.Serialize(new[] { "Posted Date", "Amount" }),
                ColumnMappings = JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    { "Posted Date", "date" },
                    { "Amount", "amount" }
                }),
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                LastUsedAt = DateTime.UtcNow.AddDays(-2),
                TimesUsed = 7,
                IsActive = true
            }
        );
        await _dbContext.SaveChangesAsync();

        var query = new GetSavedMappingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, m => m.BankIdentifier == "Chase Checking");
        Assert.Contains(result, m => m.BankIdentifier == "Bank of America");
    }

    [Fact]
    public async Task Handle_ShouldOrderByLastUsedDate()
    {
        // Arrange
        await _dbContext.ColumnMappingRules.AddRangeAsync(
            new ColumnMappingRule
            {
                Id = Guid.NewGuid(),
                BankIdentifier = "Older Mapping",
                HeaderSignature = "[]",
                ColumnMappings = "{}",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                LastUsedAt = DateTime.UtcNow.AddDays(-5),
                TimesUsed = 1,
                IsActive = true
            },
            new ColumnMappingRule
            {
                Id = Guid.NewGuid(),
                BankIdentifier = "Newer Mapping",
                HeaderSignature = "[]",
                ColumnMappings = "{}",
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                LastUsedAt = DateTime.UtcNow.AddDays(-1),
                TimesUsed = 5,
                IsActive = true
            }
        );
        await _dbContext.SaveChangesAsync();

        var query = new GetSavedMappingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Newer Mapping", result[0].BankIdentifier); // Most recent first
        Assert.Equal("Older Mapping", result[1].BankIdentifier);
    }

    [Fact]
    public async Task Handle_ShouldExcludeInactiveMappings()
    {
        // Arrange
        await _dbContext.ColumnMappingRules.AddRangeAsync(
            new ColumnMappingRule
            {
                Id = Guid.NewGuid(),
                BankIdentifier = "Active Mapping",
                HeaderSignature = "[]",
                ColumnMappings = "{}",
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow,
                IsActive = true
            },
            new ColumnMappingRule
            {
                Id = Guid.NewGuid(),
                BankIdentifier = "Inactive Mapping",
                HeaderSignature = "[]",
                ColumnMappings = "{}",
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow,
                IsActive = false
            }
        );
        await _dbContext.SaveChangesAsync();

        var query = new GetSavedMappingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal("Active Mapping", result[0].BankIdentifier);
    }

    [Fact]
    public async Task Handle_ShouldIncludeTimesUsed()
    {
        // Arrange
        await _dbContext.ColumnMappingRules.AddAsync(
            new ColumnMappingRule
            {
                Id = Guid.NewGuid(),
                BankIdentifier = "Popular Mapping",
                HeaderSignature = "[]",
                ColumnMappings = JsonSerializer.Serialize(new Dictionary<string, string>
                {
                    { "Date", "date" }
                }),
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow,
                TimesUsed = 42,
                IsActive = true
            }
        );
        await _dbContext.SaveChangesAsync();

        var query = new GetSavedMappingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(42, result[0].TimesUsed);
    }

    [Fact]
    public async Task Handle_ShouldDeserializeColumnMappings()
    {
        // Arrange
        var expectedMappings = new Dictionary<string, string>
        {
            { "Trans Date", "date" },
            { "Amount", "amount" },
            { "Description", "description" }
        };

        await _dbContext.ColumnMappingRules.AddAsync(
            new ColumnMappingRule
            {
                Id = Guid.NewGuid(),
                BankIdentifier = "Test Bank",
                HeaderSignature = "[]",
                ColumnMappings = JsonSerializer.Serialize(expectedMappings),
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow,
                IsActive = true
            }
        );
        await _dbContext.SaveChangesAsync();

        var query = new GetSavedMappingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.Single(result);
        Assert.Equal(3, result[0].ColumnMappings.Count);
        Assert.Equal("date", result[0].ColumnMappings["Trans Date"]);
        Assert.Equal("amount", result[0].ColumnMappings["Amount"]);
        Assert.Equal("description", result[0].ColumnMappings["Description"]);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
