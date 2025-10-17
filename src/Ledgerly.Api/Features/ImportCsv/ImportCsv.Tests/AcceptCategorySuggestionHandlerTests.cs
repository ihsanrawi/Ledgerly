using Ledgerly.Api.Common.Data;
using Ledgerly.Api.Common.Data.Entities;
using Ledgerly.Api.Features.ImportCsv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ImportCsv.Tests;

/// <summary>
/// Unit tests for AcceptCategorySuggestionHandler (Story 2.5).
/// Tests confidence score updates, TimesAccepted increment, and error handling.
/// </summary>
public class AcceptCategorySuggestionHandlerTests : IAsyncLifetime
{
    private readonly LedgerlyDbContext _dbContext;
    private readonly ILogger<AcceptCategorySuggestionHandler> _logger;
    private readonly AcceptCategorySuggestionHandler _sut;

    public AcceptCategorySuggestionHandlerTests()
    {
        _logger = Substitute.For<ILogger<AcceptCategorySuggestionHandler>>();

        // Setup in-memory database for tests
        var options = new DbContextOptionsBuilder<LedgerlyDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;
        _dbContext = new LedgerlyDbContext(options);

        _sut = new AcceptCategorySuggestionHandler(_dbContext, _logger);
    }

    public async Task InitializeAsync()
    {
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }

    [Fact]
    public async Task Handle_AcceptSuggestion_IncrementsTimesAccepted()
    {
        // Arrange
        var rule = new ImportRule
        {
            Id = Guid.NewGuid(),
            PayeePattern = "Whole Foods",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Exact,
            SuggestedCategory = "Expenses:Groceries",
            Priority = 1,
            Confidence = 0.80m,
            TimesApplied = 10,
            TimesAccepted = 8,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.ImportRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        var command = new AcceptCategorySuggestionCommand
        {
            RuleId = rule.Id
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.ShouldBeTrue();
        // Data integrity fix: TimesApplied also incremented, so (8+1)/(10+1) = 9/11 ≈ 0.8181818...
        result.NewConfidence.ShouldBe(9m/11m);

        var updatedRule = await _dbContext.ImportRules.FindAsync(rule.Id);
        updatedRule!.TimesAccepted.ShouldBe(9);
        updatedRule!.TimesApplied.ShouldBe(11);  // Incremented from 10 to 11
        updatedRule.Confidence.ShouldBe(9m/11m);
    }

    [Fact]
    public async Task Handle_AcceptSuggestion_RecalculatesConfidence()
    {
        // Arrange
        var rule = new ImportRule
        {
            Id = Guid.NewGuid(),
            PayeePattern = "Amazon",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains,
            SuggestedCategory = "Expenses:Shopping",
            Priority = 1,
            Confidence = 0.50m,
            TimesApplied = 20,
            TimesAccepted = 10,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.ImportRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        var command = new AcceptCategorySuggestionCommand
        {
            RuleId = rule.Id
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.ShouldBeTrue();
        // Data integrity fix: TimesApplied also incremented, so (10+1)/(20+1) = 11/21 ≈ 0.5238...
        result.NewConfidence.ShouldBe(11m/21m);

        var updatedRule = await _dbContext.ImportRules.FindAsync(rule.Id);
        updatedRule!.TimesAccepted.ShouldBe(11);
        updatedRule!.TimesApplied.ShouldBe(21);  // Incremented from 20 to 21
        updatedRule.Confidence.ShouldBe(11m/21m);
    }

    [Fact]
    public async Task Handle_AcceptSuggestion_ConfidenceApproaches100Percent()
    {
        // Arrange
        var rule = new ImportRule
        {
            Id = Guid.NewGuid(),
            PayeePattern = "Netflix",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains,
            SuggestedCategory = "Expenses:Entertainment",
            Priority = 1,
            Confidence = 0.90m,
            TimesApplied = 10,
            TimesAccepted = 9,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.ImportRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        var command = new AcceptCategorySuggestionCommand
        {
            RuleId = rule.Id
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.ShouldBeTrue();
        // Data integrity fix: TimesApplied also incremented, so (9+1)/(10+1) = 10/11 ≈ 0.909...
        result.NewConfidence.ShouldBe(10m/11m);

        var updatedRule = await _dbContext.ImportRules.FindAsync(rule.Id);
        updatedRule!.TimesAccepted.ShouldBe(10);
        updatedRule!.TimesApplied.ShouldBe(11);  // Incremented from 10 to 11
        updatedRule.Confidence.ShouldBe(10m/11m);
    }

    [Fact]
    public async Task Handle_AcceptSuggestion_MultipleAcceptances()
    {
        // Arrange
        var rule = new ImportRule
        {
            Id = Guid.NewGuid(),
            PayeePattern = "Starbucks",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.StartsWith,
            SuggestedCategory = "Expenses:Coffee",
            Priority = 1,
            Confidence = 0.60m,
            TimesApplied = 5,
            TimesAccepted = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.ImportRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        var command = new AcceptCategorySuggestionCommand
        {
            RuleId = rule.Id
        };

        // Act - Accept three times
        var result1 = await _sut.Handle(command, CancellationToken.None);
        var result2 = await _sut.Handle(command, CancellationToken.None);
        var result3 = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result1.Success.ShouldBeTrue();
        result2.Success.ShouldBeTrue();
        result3.Success.ShouldBeTrue();

        var updatedRule = await _dbContext.ImportRules.FindAsync(rule.Id);
        updatedRule!.TimesAccepted.ShouldBe(6);
        updatedRule!.TimesApplied.ShouldBe(8);  // Incremented 3 times from 5 to 8
        // Data integrity fix: (3+3)/(5+3) = 6/8 = 0.75
        updatedRule.Confidence.ShouldBe(0.75m);
    }

    [Fact]
    public async Task Handle_RuleNotFound_ThrowsException()
    {
        // Arrange
        var nonExistentRuleId = Guid.NewGuid();
        var command = new AcceptCategorySuggestionCommand
        {
            RuleId = nonExistentRuleId
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(async () =>
            await _sut.Handle(command, CancellationToken.None));

        exception.Message.ShouldContain(nonExistentRuleId.ToString());
    }

    [Fact]
    public async Task Handle_AcceptSuggestion_ZeroTimesApplied_HandlesGracefully()
    {
        // Arrange - Edge case: rule never applied (shouldn't happen, but test defensive code)
        var rule = new ImportRule
        {
            Id = Guid.NewGuid(),
            PayeePattern = "Test",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Exact,
            SuggestedCategory = "Expenses:Test",
            Priority = 1,
            Confidence = 0.0m,
            TimesApplied = 0,
            TimesAccepted = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.ImportRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        var command = new AcceptCategorySuggestionCommand
        {
            RuleId = rule.Id
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.ShouldBeTrue();
        // Data integrity fix: TimesApplied also incremented, so (0+1)/(0+1) = 1/1 = 1.0 (100% confidence)
        result.NewConfidence.ShouldBe(1.0m);

        var updatedRule = await _dbContext.ImportRules.FindAsync(rule.Id);
        updatedRule!.TimesAccepted.ShouldBe(1);
        updatedRule!.TimesApplied.ShouldBe(1);  // Incremented from 0 to 1
        updatedRule.Confidence.ShouldBe(1.0m);
    }

    [Fact]
    public async Task Handle_AcceptSuggestion_LowConfidenceImprovement()
    {
        // Arrange
        var rule = new ImportRule
        {
            Id = Guid.NewGuid(),
            PayeePattern = "Uncertain Merchant",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains,
            SuggestedCategory = "Expenses:Unknown",
            Priority = 1,
            Confidence = 0.10m,
            TimesApplied = 10,
            TimesAccepted = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.ImportRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        var command = new AcceptCategorySuggestionCommand
        {
            RuleId = rule.Id
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Success.ShouldBeTrue();
        // Data integrity fix: TimesApplied also incremented, so (1+1)/(10+1) = 2/11 ≈ 0.1818...
        result.NewConfidence.ShouldBe(2m/11m);

        var updatedRule = await _dbContext.ImportRules.FindAsync(rule.Id);
        updatedRule!.TimesAccepted.ShouldBe(2);
        updatedRule!.TimesApplied.ShouldBe(11);  // Incremented from 10 to 11
        updatedRule.Confidence.ShouldBe(2m/11m);
    }
}
