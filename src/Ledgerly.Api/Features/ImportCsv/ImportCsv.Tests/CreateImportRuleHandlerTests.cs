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
/// Unit tests for CreateImportRuleHandler (Story 2.5).
/// Tests new rule creation, validation, and initial confidence settings.
/// </summary>
public class CreateImportRuleHandlerTests : IAsyncLifetime
{
    private readonly LedgerlyDbContext _dbContext;
    private readonly ILogger<CreateImportRuleHandler> _logger;
    private readonly CreateImportRuleHandler _sut;

    public CreateImportRuleHandlerTests()
    {
        _logger = Substitute.For<ILogger<CreateImportRuleHandler>>();

        // Setup in-memory database for tests
        var options = new DbContextOptionsBuilder<LedgerlyDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;
        _dbContext = new LedgerlyDbContext(options);

        _sut = new CreateImportRuleHandler(_dbContext, _logger);
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
    public async Task Handle_CreateNewRule_SuccessfullyCreates()
    {
        // Arrange
        var command = new CreateImportRuleCommand
        {
            PayeePattern = "Whole Foods",
            SuggestedCategory = "Expenses:Groceries",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.RuleId.ShouldNotBe(Guid.Empty);
        result.Message.ShouldBe("Rule created successfully");

        var createdRule = await _dbContext.ImportRules.FindAsync(result.RuleId);
        createdRule.ShouldNotBeNull();
        createdRule!.PayeePattern.ShouldBe("Whole Foods");
        createdRule.SuggestedCategory.ShouldBe("Expenses:Groceries");
        createdRule.MatchType.ShouldBe(Ledgerly.Api.Common.Data.Entities.MatchType.Contains);
        createdRule.Priority.ShouldBe(1); // Highest priority for user-created
        createdRule.Confidence.ShouldBe(0.6m); // Initial confidence
        createdRule.TimesApplied.ShouldBe(1);
        createdRule.TimesAccepted.ShouldBe(1);
        createdRule.IsActive.ShouldBeTrue();
        createdRule.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(1));
        createdRule.LastUsedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_CreateRuleWithExactMatch_SuccessfullyCreates()
    {
        // Arrange
        var command = new CreateImportRuleCommand
        {
            PayeePattern = "AMAZON.COM",
            SuggestedCategory = "Expenses:Shopping",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Exact
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        var createdRule = await _dbContext.ImportRules.FindAsync(result.RuleId);
        createdRule!.MatchType.ShouldBe(Ledgerly.Api.Common.Data.Entities.MatchType.Exact);
    }

    [Fact]
    public async Task Handle_CreateRuleWithStartsWith_SuccessfullyCreates()
    {
        // Arrange
        var command = new CreateImportRuleCommand
        {
            PayeePattern = "STARBUCKS",
            SuggestedCategory = "Expenses:Coffee",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.StartsWith
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        var createdRule = await _dbContext.ImportRules.FindAsync(result.RuleId);
        createdRule!.MatchType.ShouldBe(Ledgerly.Api.Common.Data.Entities.MatchType.StartsWith);
    }

    [Fact]
    public async Task Handle_CreateRuleWithEndsWith_SuccessfullyCreates()
    {
        // Arrange
        var command = new CreateImportRuleCommand
        {
            PayeePattern = ".COM",
            SuggestedCategory = "Expenses:Online",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.EndsWith
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        var createdRule = await _dbContext.ImportRules.FindAsync(result.RuleId);
        createdRule!.MatchType.ShouldBe(Ledgerly.Api.Common.Data.Entities.MatchType.EndsWith);
    }

    [Fact]
    public async Task Handle_EmptyPayeePattern_ThrowsException()
    {
        // Arrange
        var command = new CreateImportRuleCommand
        {
            PayeePattern = "",
            SuggestedCategory = "Expenses:Test",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await _sut.Handle(command, CancellationToken.None));

        exception.Message.ShouldContain("Payee pattern cannot be empty");
    }

    [Fact]
    public async Task Handle_WhitespacePayeePattern_ThrowsException()
    {
        // Arrange
        var command = new CreateImportRuleCommand
        {
            PayeePattern = "   ",
            SuggestedCategory = "Expenses:Test",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await _sut.Handle(command, CancellationToken.None));

        exception.Message.ShouldContain("Payee pattern cannot be empty");
    }

    [Fact]
    public async Task Handle_EmptySuggestedCategory_ThrowsException()
    {
        // Arrange
        var command = new CreateImportRuleCommand
        {
            PayeePattern = "Test Merchant",
            SuggestedCategory = "",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await _sut.Handle(command, CancellationToken.None));

        exception.Message.ShouldContain("Suggested category cannot be empty");
    }

    [Fact]
    public async Task Handle_WhitespaceSuggestedCategory_ThrowsException()
    {
        // Arrange
        var command = new CreateImportRuleCommand
        {
            PayeePattern = "Test Merchant",
            SuggestedCategory = "   ",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<ArgumentException>(async () =>
            await _sut.Handle(command, CancellationToken.None));

        exception.Message.ShouldContain("Suggested category cannot be empty");
    }

    [Fact]
    public async Task Handle_PayeePatternWithWhitespace_TrimsWhitespace()
    {
        // Arrange
        var command = new CreateImportRuleCommand
        {
            PayeePattern = "  Whole Foods  ",
            SuggestedCategory = "  Expenses:Groceries  ",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        var createdRule = await _dbContext.ImportRules.FindAsync(result.RuleId);
        createdRule!.PayeePattern.ShouldBe("Whole Foods");
        createdRule.SuggestedCategory.ShouldBe("Expenses:Groceries");
    }

    [Fact]
    public async Task Handle_MultipleRulesCreated_AllHavePriority1()
    {
        // Arrange
        var command1 = new CreateImportRuleCommand
        {
            PayeePattern = "Merchant 1",
            SuggestedCategory = "Expenses:Category1",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains
        };

        var command2 = new CreateImportRuleCommand
        {
            PayeePattern = "Merchant 2",
            SuggestedCategory = "Expenses:Category2",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains
        };

        // Act
        var result1 = await _sut.Handle(command1, CancellationToken.None);
        var result2 = await _sut.Handle(command2, CancellationToken.None);

        // Assert
        var rule1 = await _dbContext.ImportRules.FindAsync(result1.RuleId);
        var rule2 = await _dbContext.ImportRules.FindAsync(result2.RuleId);

        rule1!.Priority.ShouldBe(1);
        rule2!.Priority.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_CreateRule_InitialConfidenceIs60Percent()
    {
        // Arrange
        var command = new CreateImportRuleCommand
        {
            PayeePattern = "Test Merchant",
            SuggestedCategory = "Expenses:Test",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        var createdRule = await _dbContext.ImportRules.FindAsync(result.RuleId);
        createdRule!.Confidence.ShouldBe(0.6m);
        createdRule.TimesApplied.ShouldBe(1);
        createdRule.TimesAccepted.ShouldBe(1);
        // This ensures 1/1 could give 1.0, but we set initial to 0.6 per spec
    }

    [Fact]
    public async Task Handle_CreateRule_IsActiveByDefault()
    {
        // Arrange
        var command = new CreateImportRuleCommand
        {
            PayeePattern = "Test Merchant",
            SuggestedCategory = "Expenses:Test",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains
        };

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        var createdRule = await _dbContext.ImportRules.FindAsync(result.RuleId);
        createdRule!.IsActive.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_CreateRule_SetsCreatedAtAndLastUsedAt()
    {
        // Arrange
        var command = new CreateImportRuleCommand
        {
            PayeePattern = "Test Merchant",
            SuggestedCategory = "Expenses:Test",
            MatchType = Ledgerly.Api.Common.Data.Entities.MatchType.Contains
        };

        var beforeCreate = DateTime.UtcNow;

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        var afterCreate = DateTime.UtcNow;
        var createdRule = await _dbContext.ImportRules.FindAsync(result.RuleId);

        createdRule!.CreatedAt.ShouldBeInRange(beforeCreate, afterCreate);
        createdRule.LastUsedAt.ShouldNotBeNull();
        createdRule.LastUsedAt.Value.ShouldBeInRange(beforeCreate, afterCreate);
    }
}
