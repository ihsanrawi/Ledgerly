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
/// Unit tests for GetCategorySuggestionsHandler (Story 2.5).
/// Tests ImportRule pattern matching, priority ordering, and confidence scoring.
/// </summary>
public class GetCategorySuggestionsHandlerTests : IAsyncLifetime
{
    private readonly LedgerlyDbContext _dbContext;
    private readonly ILogger<GetCategorySuggestionsHandler> _logger;
    private readonly GetCategorySuggestionsHandler _sut;

    public GetCategorySuggestionsHandlerTests()
    {
        _logger = Substitute.For<ILogger<GetCategorySuggestionsHandler>>();

        // Setup in-memory database for tests
        var options = new DbContextOptionsBuilder<LedgerlyDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;
        _dbContext = new LedgerlyDbContext(options);

        _sut = new GetCategorySuggestionsHandler(_dbContext, _logger);
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
    public async Task Handle_ExactMatch_ReturnsSuggestion()
    {
        // Arrange
        var rule = new ImportRule
        {
            Id = Guid.NewGuid(),
            PayeePattern = "Whole Foods",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Exact,
            SuggestedCategory = "Expenses:Groceries",
            Priority = 1,
            Confidence = 0.85m,
            TimesApplied = 10,
            TimesAccepted = 8,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.ImportRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        var query = new GetCategorySuggestionsQuery
        {
            Transactions = new List<ParsedTransactionDto>
            {
                new()
                {
                    Date = new DateTime(2025, 1, 15),
                    Payee = "Whole Foods",
                    Amount = 42.50m,
                    RowIndex = 0
                }
            }
        };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Suggestions.Count.ShouldBe(1);
        result.Suggestions[0].TransactionIndex.ShouldBe(0);
        result.Suggestions[0].RuleId.ShouldBe(rule.Id);
        result.Suggestions[0].SuggestedCategory.ShouldBe("Expenses:Groceries");
        result.Suggestions[0].Confidence.ShouldBe(0.85m);
        result.Suggestions[0].MatchedPattern.ShouldBe("Whole Foods");

        // Data integrity fix: TimesApplied NO LONGER incremented during suggestion phase
        // TimesApplied is only incremented when user accepts the suggestion (AcceptCategorySuggestionHandler)
        var updatedRule = await _dbContext.ImportRules.FindAsync(rule.Id);
        updatedRule!.TimesApplied.ShouldBe(10);  // Should remain unchanged (not incremented from 10 to 11)
    }

    [Fact]
    public async Task Handle_ContainsMatch_ReturnsSuggestion()
    {
        // Arrange
        var rule = new ImportRule
        {
            Id = Guid.NewGuid(),
            PayeePattern = "AMAZON",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains,
            SuggestedCategory = "Expenses:Shopping",
            Priority = 1,
            Confidence = 0.90m,
            TimesApplied = 20,
            TimesAccepted = 18,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.ImportRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        var query = new GetCategorySuggestionsQuery
        {
            Transactions = new List<ParsedTransactionDto>
            {
                new()
                {
                    Date = new DateTime(2025, 1, 15),
                    Payee = "AMAZON MKTP US*AB1234567",
                    Amount = 29.99m,
                    RowIndex = 0
                }
            }
        };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Suggestions.Count.ShouldBe(1);
        result.Suggestions[0].SuggestedCategory.ShouldBe("Expenses:Shopping");
        result.Suggestions[0].MatchedPattern.ShouldBe("AMAZON");
    }

    [Fact]
    public async Task Handle_StartsWithMatch_ReturnsSuggestion()
    {
        // Arrange
        var rule = new ImportRule
        {
            Id = Guid.NewGuid(),
            PayeePattern = "STARBUCKS",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.StartsWith,
            SuggestedCategory = "Expenses:Coffee",
            Priority = 1,
            Confidence = 0.75m,
            TimesApplied = 8,
            TimesAccepted = 6,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.ImportRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        var query = new GetCategorySuggestionsQuery
        {
            Transactions = new List<ParsedTransactionDto>
            {
                new()
                {
                    Date = new DateTime(2025, 1, 15),
                    Payee = "STARBUCKS STORE #12345",
                    Amount = 5.50m,
                    RowIndex = 0
                }
            }
        };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Suggestions.Count.ShouldBe(1);
        result.Suggestions[0].SuggestedCategory.ShouldBe("Expenses:Coffee");
    }

    [Fact]
    public async Task Handle_EndsWithMatch_ReturnsSuggestion()
    {
        // Arrange
        var rule = new ImportRule
        {
            Id = Guid.NewGuid(),
            PayeePattern = "NETFLIX.COM",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.EndsWith,
            SuggestedCategory = "Expenses:Entertainment",
            Priority = 1,
            Confidence = 0.95m,
            TimesApplied = 12,
            TimesAccepted = 11,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.ImportRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        var query = new GetCategorySuggestionsQuery
        {
            Transactions = new List<ParsedTransactionDto>
            {
                new()
                {
                    Date = new DateTime(2025, 1, 15),
                    Payee = "SUBSCRIPTION NETFLIX.COM",
                    Amount = 15.99m,
                    RowIndex = 0
                }
            }
        };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Suggestions.Count.ShouldBe(1);
        result.Suggestions[0].SuggestedCategory.ShouldBe("Expenses:Entertainment");
    }

    [Fact]
    public async Task Handle_CaseInsensitiveMatching_ReturnsSuggestion()
    {
        // Arrange
        var rule = new ImportRule
        {
            Id = Guid.NewGuid(),
            PayeePattern = "whole foods",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Exact,
            SuggestedCategory = "Expenses:Groceries",
            Priority = 1,
            Confidence = 0.80m,
            TimesApplied = 5,
            TimesAccepted = 4,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.ImportRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        var query = new GetCategorySuggestionsQuery
        {
            Transactions = new List<ParsedTransactionDto>
            {
                new()
                {
                    Date = new DateTime(2025, 1, 15),
                    Payee = "WHOLE FOODS", // Uppercase
                    Amount = 42.50m,
                    RowIndex = 0
                }
            }
        };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Suggestions.Count.ShouldBe(1);
        result.Suggestions[0].SuggestedCategory.ShouldBe("Expenses:Groceries");
    }

    [Fact]
    public async Task Handle_PriorityOrdering_HigherPriorityWins()
    {
        // Arrange
        var lowPriorityRule = new ImportRule
        {
            Id = Guid.NewGuid(),
            PayeePattern = "AMAZON",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains,
            SuggestedCategory = "Expenses:Shopping",
            Priority = 10, // Lower priority
            Confidence = 0.90m,
            TimesApplied = 20,
            TimesAccepted = 18,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var highPriorityRule = new ImportRule
        {
            Id = Guid.NewGuid(),
            PayeePattern = "AMAZON PRIME",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains,
            SuggestedCategory = "Expenses:Subscriptions",
            Priority = 1, // Higher priority
            Confidence = 0.95m,
            TimesApplied = 15,
            TimesAccepted = 14,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ImportRules.AddRange(lowPriorityRule, highPriorityRule);
        await _dbContext.SaveChangesAsync();

        var query = new GetCategorySuggestionsQuery
        {
            Transactions = new List<ParsedTransactionDto>
            {
                new()
                {
                    Date = new DateTime(2025, 1, 15),
                    Payee = "AMAZON PRIME MEMBERSHIP",
                    Amount = 14.99m,
                    RowIndex = 0
                }
            }
        };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Suggestions.Count.ShouldBe(1);
        result.Suggestions[0].RuleId.ShouldBe(highPriorityRule.Id);
        result.Suggestions[0].SuggestedCategory.ShouldBe("Expenses:Subscriptions");
        result.Suggestions[0].MatchedPattern.ShouldBe("AMAZON PRIME");
    }

    [Fact]
    public async Task Handle_NoMatch_ReturnsNoSuggestions()
    {
        // Arrange
        var rule = new ImportRule
        {
            Id = Guid.NewGuid(),
            PayeePattern = "Whole Foods",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Exact,
            SuggestedCategory = "Expenses:Groceries",
            Priority = 1,
            Confidence = 0.85m,
            TimesApplied = 10,
            TimesAccepted = 8,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.ImportRules.Add(rule);
        await _dbContext.SaveChangesAsync();

        var query = new GetCategorySuggestionsQuery
        {
            Transactions = new List<ParsedTransactionDto>
            {
                new()
                {
                    Date = new DateTime(2025, 1, 15),
                    Payee = "Unknown Merchant",
                    Amount = 42.50m,
                    RowIndex = 0
                }
            }
        };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Suggestions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_InactiveRule_IgnoresRule()
    {
        // Arrange
        var inactiveRule = new ImportRule
        {
            Id = Guid.NewGuid(),
            PayeePattern = "Whole Foods",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Exact,
            SuggestedCategory = "Expenses:Groceries",
            Priority = 1,
            Confidence = 0.85m,
            TimesApplied = 10,
            TimesAccepted = 8,
            IsActive = false, // Inactive
            CreatedAt = DateTime.UtcNow
        };
        _dbContext.ImportRules.Add(inactiveRule);
        await _dbContext.SaveChangesAsync();

        var query = new GetCategorySuggestionsQuery
        {
            Transactions = new List<ParsedTransactionDto>
            {
                new()
                {
                    Date = new DateTime(2025, 1, 15),
                    Payee = "Whole Foods",
                    Amount = 42.50m,
                    RowIndex = 0
                }
            }
        };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Suggestions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_NoRules_ReturnsNoSuggestions()
    {
        // Arrange
        var query = new GetCategorySuggestionsQuery
        {
            Transactions = new List<ParsedTransactionDto>
            {
                new()
                {
                    Date = new DateTime(2025, 1, 15),
                    Payee = "Whole Foods",
                    Amount = 42.50m,
                    RowIndex = 0
                }
            }
        };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Suggestions.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_MultipleTransactions_ReturnsMultipleSuggestions()
    {
        // Arrange
        var rules = new[]
        {
            new ImportRule
            {
                Id = Guid.NewGuid(),
                PayeePattern = "Whole Foods",
                MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains,
                SuggestedCategory = "Expenses:Groceries",
                Priority = 1,
                Confidence = 0.85m,
                TimesApplied = 10,
                TimesAccepted = 8,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ImportRule
            {
                Id = Guid.NewGuid(),
                PayeePattern = "AMAZON",
                MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains,
                SuggestedCategory = "Expenses:Shopping",
                Priority = 2,
                Confidence = 0.90m,
                TimesApplied = 20,
                TimesAccepted = 18,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ImportRule
            {
                Id = Guid.NewGuid(),
                PayeePattern = "NETFLIX",
                MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains,
                SuggestedCategory = "Expenses:Entertainment",
                Priority = 3,
                Confidence = 0.95m,
                TimesApplied = 12,
                TimesAccepted = 11,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
        _dbContext.ImportRules.AddRange(rules);
        await _dbContext.SaveChangesAsync();

        var query = new GetCategorySuggestionsQuery
        {
            Transactions = new List<ParsedTransactionDto>
            {
                new()
                {
                    Date = new DateTime(2025, 1, 15),
                    Payee = "Whole Foods Market",
                    Amount = 42.50m,
                    RowIndex = 0
                },
                new()
                {
                    Date = new DateTime(2025, 1, 16),
                    Payee = "AMAZON MKTP",
                    Amount = 29.99m,
                    RowIndex = 1
                },
                new()
                {
                    Date = new DateTime(2025, 1, 17),
                    Payee = "NETFLIX.COM",
                    Amount = 15.99m,
                    RowIndex = 2
                },
                new()
                {
                    Date = new DateTime(2025, 1, 18),
                    Payee = "Unknown Merchant",
                    Amount = 10.00m,
                    RowIndex = 3
                }
            }
        };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Suggestions.Count.ShouldBe(3);
        result.Suggestions.Any(s => s.TransactionIndex == 0 && s.SuggestedCategory == "Expenses:Groceries").ShouldBeTrue();
        result.Suggestions.Any(s => s.TransactionIndex == 1 && s.SuggestedCategory == "Expenses:Shopping").ShouldBeTrue();
        result.Suggestions.Any(s => s.TransactionIndex == 2 && s.SuggestedCategory == "Expenses:Entertainment").ShouldBeTrue();
        result.Suggestions.Any(s => s.TransactionIndex == 3).ShouldBeFalse();
    }

    [Fact]
    public async Task Handle_ConfidenceScoring_ReturnsCorrectValues()
    {
        // Arrange
        var highConfidenceRule = new ImportRule
        {
            Id = Guid.NewGuid(),
            PayeePattern = "Netflix",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains,
            SuggestedCategory = "Expenses:Entertainment",
            Priority = 1,
            Confidence = 1.0m, // Perfect confidence
            TimesApplied = 10,
            TimesAccepted = 10,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var lowConfidenceRule = new ImportRule
        {
            Id = Guid.NewGuid(),
            PayeePattern = "Generic Store",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains,
            SuggestedCategory = "Expenses:Shopping",
            Priority = 2,
            Confidence = 0.3m, // Low confidence
            TimesApplied = 10,
            TimesAccepted = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.ImportRules.AddRange(highConfidenceRule, lowConfidenceRule);
        await _dbContext.SaveChangesAsync();

        var query = new GetCategorySuggestionsQuery
        {
            Transactions = new List<ParsedTransactionDto>
            {
                new()
                {
                    Date = new DateTime(2025, 1, 15),
                    Payee = "Netflix Subscription",
                    Amount = 15.99m,
                    RowIndex = 0
                },
                new()
                {
                    Date = new DateTime(2025, 1, 16),
                    Payee = "Generic Store #123",
                    Amount = 25.00m,
                    RowIndex = 1
                }
            }
        };

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        result.Suggestions.Count.ShouldBe(2);
        result.Suggestions.First(s => s.TransactionIndex == 0).Confidence.ShouldBe(1.0m);
        result.Suggestions.First(s => s.TransactionIndex == 1).Confidence.ShouldBe(0.3m);
    }
}
