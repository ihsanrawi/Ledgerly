using System.Text.Json;
using Ledgerly.Api.Common.Data;
using Ledgerly.Api.Common.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Ledgerly.Api.Features.ImportCsv.Tests;

/// <summary>
/// Unit tests for SaveColumnMappingHandler.
/// Story 2.4 - Manual Column Mapping Interface (AC: 5, 6).
/// </summary>
public class SaveColumnMappingHandlerTests : IDisposable
{
    private readonly LedgerlyDbContext _dbContext;
    private readonly ILogger<SaveColumnMappingHandler> _logger;
    private readonly SaveColumnMappingHandler _handler;

    public SaveColumnMappingHandlerTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<LedgerlyDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;
        _dbContext = new LedgerlyDbContext(options);

        _logger = Substitute.For<ILogger<SaveColumnMappingHandler>>();
        _handler = new SaveColumnMappingHandler(_dbContext, _logger);
    }

    [Fact]
    public async Task Handle_NewMapping_ShouldCreateMapping()
    {
        // Arrange
        var command = new SaveColumnMappingCommand
        {
            BankIdentifier = "Chase Checking",
            ColumnMappings = new Dictionary<string, string>
            {
                { "Trans Date", "date" },
                { "Amount", "amount" },
                { "Description", "description" }
            },
            HeaderSignature = new[] { "Trans Date", "Amount", "Description", "Balance" }
        };

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, response.Id);
        Assert.Contains("Chase Checking", response.Message);

        var savedMapping = await _dbContext.ColumnMappingRules.FirstOrDefaultAsync();
        Assert.NotNull(savedMapping);
        Assert.Equal("Chase Checking", savedMapping.BankIdentifier);
        Assert.Equal(0, savedMapping.TimesUsed);
        Assert.True(savedMapping.IsActive);
    }

    [Fact]
    public async Task Handle_ExistingMapping_ShouldUpdateMapping()
    {
        // Arrange
        var existingMapping = new ColumnMappingRule
        {
            Id = Guid.NewGuid(),
            BankIdentifier = "Chase Checking",
            HeaderSignature = JsonSerializer.Serialize(new[] { "Date", "Amount", "Payee" }),
            ColumnMappings = JsonSerializer.Serialize(new Dictionary<string, string>
            {
                { "Date", "date" },
                { "Amount", "amount" }
            }),
            CreatedAt = DateTime.UtcNow.AddDays(-7),
            LastUsedAt = DateTime.UtcNow.AddDays(-7),
            TimesUsed = 5,
            IsActive = true
        };

        await _dbContext.ColumnMappingRules.AddAsync(existingMapping);
        await _dbContext.SaveChangesAsync();

        var command = new SaveColumnMappingCommand
        {
            BankIdentifier = "Chase Checking",
            ColumnMappings = new Dictionary<string, string>
            {
                { "Trans Date", "date" },
                { "Amount", "amount" },
                { "Description", "description" }
            },
            HeaderSignature = new[] { "Trans Date", "Amount", "Description" }
        };

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(existingMapping.Id, response.Id);

        var updatedMapping = await _dbContext.ColumnMappingRules.FirstOrDefaultAsync();
        Assert.NotNull(updatedMapping);
        Assert.Equal("Chase Checking", updatedMapping.BankIdentifier);
        Assert.Equal(5, updatedMapping.TimesUsed); // Should preserve times used

        var updatedMappings = JsonSerializer.Deserialize<Dictionary<string, string>>(updatedMapping.ColumnMappings);
        Assert.NotNull(updatedMappings);
        Assert.Equal(3, updatedMappings.Count);
        Assert.Equal("description", updatedMappings["Description"]);
    }

    [Fact]
    public async Task Handle_WithFileNamePattern_ShouldSavePattern()
    {
        // Arrange
        var command = new SaveColumnMappingCommand
        {
            BankIdentifier = "Bank of America",
            ColumnMappings = new Dictionary<string, string>
            {
                { "Posted Date", "date" },
                { "Payee", "description" },
                { "Amount", "amount" }
            },
            FileNamePattern = "*bofa*.csv",
            HeaderSignature = new[] { "Posted Date", "Payee", "Amount" }
        };

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedMapping = await _dbContext.ColumnMappingRules.FirstOrDefaultAsync();
        Assert.NotNull(savedMapping);
        Assert.Equal("*bofa*.csv", savedMapping.BankMatchPattern);
    }

    [Fact]
    public async Task Handle_WithDebitCreditColumns_ShouldSaveCorrectly()
    {
        // Arrange - CSV with split debit/credit columns
        var command = new SaveColumnMappingCommand
        {
            BankIdentifier = "Wells Fargo",
            ColumnMappings = new Dictionary<string, string>
            {
                { "Date", "date" },
                { "Debit", "debit" },
                { "Credit", "credit" },
                { "Description", "description" }
            },
            HeaderSignature = new[] { "Date", "Debit", "Credit", "Description" }
        };

        // Act
        var response = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var savedMapping = await _dbContext.ColumnMappingRules.FirstOrDefaultAsync();
        Assert.NotNull(savedMapping);

        var mappings = JsonSerializer.Deserialize<Dictionary<string, string>>(savedMapping.ColumnMappings);
        Assert.NotNull(mappings);
        Assert.Equal("debit", mappings["Debit"]);
        Assert.Equal("credit", mappings["Credit"]);
    }

    [Fact]
    public async Task Handle_MultipleActiveMappings_ShouldKeepAll()
    {
        // Arrange
        await _dbContext.ColumnMappingRules.AddRangeAsync(
            new ColumnMappingRule
            {
                Id = Guid.NewGuid(),
                BankIdentifier = "Chase Checking",
                HeaderSignature = JsonSerializer.Serialize(new[] { "Date", "Amount" }),
                ColumnMappings = "{}",
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow,
                IsActive = true
            },
            new ColumnMappingRule
            {
                Id = Guid.NewGuid(),
                BankIdentifier = "Bank of America",
                HeaderSignature = JsonSerializer.Serialize(new[] { "Posted Date", "Amount" }),
                ColumnMappings = "{}",
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow,
                IsActive = true
            }
        );
        await _dbContext.SaveChangesAsync();

        var command = new SaveColumnMappingCommand
        {
            BankIdentifier = "Wells Fargo",
            ColumnMappings = new Dictionary<string, string> { { "Date", "date" }, { "Amount", "amount" } },
            HeaderSignature = new[] { "Date", "Amount" }
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var allMappings = await _dbContext.ColumnMappingRules.Where(r => r.IsActive).ToListAsync();
        Assert.Equal(3, allMappings.Count);
    }

    public void Dispose()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }
}
