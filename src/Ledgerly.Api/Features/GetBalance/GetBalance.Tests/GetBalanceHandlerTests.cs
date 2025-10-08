using Ledgerly.Api.Common.Hledger;
using Ledgerly.Api.Features.GetBalance;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace GetBalance.Tests;

/// <summary>
/// Unit tests for GetBalanceHandler.
/// Tests BuildHierarchy algorithm and handler logic with mocked HledgerProcessRunner.
/// </summary>
public class GetBalanceHandlerTests
{
    private readonly IHledgerProcessRunner _mockProcessRunner;
    private readonly ILogger<GetBalanceHandler> _mockLogger;
    private readonly GetBalanceHandler _handler;

    public GetBalanceHandlerTests()
    {
        _mockProcessRunner = Substitute.For<IHledgerProcessRunner>();
        _mockLogger = Substitute.For<ILogger<GetBalanceHandler>>();
        _handler = new GetBalanceHandler(_mockProcessRunner, _mockLogger);
    }

    [Fact]
    public async Task Handle_NoFilters_ReturnsAllAccounts()
    {
        // Arrange
        var balances = new List<BalanceEntry>
        {
            new() { Account = "Assets:Checking", Amount = 1000m, Commodity = "$" },
            new() { Account = "Expenses:Groceries", Amount = 250m, Commodity = "$" }
        };

        _mockProcessRunner
            .GetBalances(Arg.Any<string>(), Arg.Any<string[]?>())
            .Returns(new HledgerBalanceResult
            {
                Balances = balances,
                TotalBalance = 1250m
            });

        var query = new GetBalanceQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Balances.ShouldNotBeEmpty();
        result.AsOfDate.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
    }

    [Fact]
    public async Task Handle_WithAccountFilter_PassesFilterToProcessRunner()
    {
        // Arrange
        var balances = new List<BalanceEntry>
        {
            new() { Account = "Assets:Checking", Amount = 1000m, Commodity = "$" }
        };

        _mockProcessRunner
            .GetBalances(Arg.Any<string>(), Arg.Any<string[]?>())
            .Returns(new HledgerBalanceResult
            {
                Balances = balances,
                TotalBalance = 1000m
            });

        var query = new GetBalanceQuery("Assets,Expenses");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        await _mockProcessRunner.Received(1).GetBalances(
            Arg.Any<string>(),
            Arg.Is<string[]?>(arr => arr != null && arr.Length == 2 && arr[0] == "Assets" && arr[1] == "Expenses"));
    }

    [Fact]
    public async Task BuildHierarchy_FlatList_CreatesCorrectTree()
    {
        // Arrange
        var balances = new List<BalanceEntry>
        {
            new() { Account = "Assets:Checking", Amount = 1000m, Commodity = "$" },
            new() { Account = "Assets:Savings", Amount = 5000m, Commodity = "$" },
            new() { Account = "Expenses:Groceries", Amount = 250m, Commodity = "$" }
        };

        _mockProcessRunner
            .GetBalances(Arg.Any<string>(), Arg.Any<string[]?>())
            .Returns(new HledgerBalanceResult
            {
                Balances = balances,
                TotalBalance = 6250m
            });

        var query = new GetBalanceQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Balances.Count.ShouldBe(2); // Assets, Expenses (root level stubs)

        var assets = result.Balances.FirstOrDefault(b => b.Account == "Assets");
        assets.ShouldNotBeNull();
        assets.Children.Count.ShouldBe(2); // Checking, Savings
        assets.Balance.ShouldBe(6000m); // Stub node - sum of children (1000 + 5000)

        var expenses = result.Balances.FirstOrDefault(b => b.Account == "Expenses");
        expenses.ShouldNotBeNull();
        expenses.Children.Count.ShouldBe(1); // Groceries
        expenses.Balance.ShouldBe(250m); // Stub node - sum of children
    }

    [Fact]
    public async Task BuildHierarchy_MissingParent_CreatesStubNode()
    {
        // Arrange
        var balances = new List<BalanceEntry>
        {
            new() { Account = "Expenses:Dining:Restaurants", Amount = 75m, Commodity = "$" }
        };

        _mockProcessRunner
            .GetBalances(Arg.Any<string>(), Arg.Any<string[]?>())
            .Returns(new HledgerBalanceResult
            {
                Balances = balances,
                TotalBalance = 75m
            });

        var query = new GetBalanceQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Balances.Count.ShouldBe(1); // Expenses (stub root)

        var expenses = result.Balances[0];
        expenses.Account.ShouldBe("Expenses");
        expenses.Balance.ShouldBe(75m); // Stub - sum of descendants
        expenses.Children.Count.ShouldBe(1); // Dining (stub)

        var dining = expenses.Children[0];
        dining.Account.ShouldBe("Expenses:Dining");
        dining.Balance.ShouldBe(75m); // Stub - sum of children
        dining.Children.Count.ShouldBe(1); // Restaurants (actual data)

        var restaurants = dining.Children[0];
        restaurants.Account.ShouldBe("Expenses:Dining:Restaurants");
        restaurants.Balance.ShouldBe(75m); // Actual balance
        restaurants.Children.Count.ShouldBe(0); // Leaf node
    }

    [Fact]
    public async Task BuildHierarchy_SingleLevelAccounts_AddedToRootList()
    {
        // Arrange
        var balances = new List<BalanceEntry>
        {
            new() { Account = "Cash", Amount = 500m, Commodity = "$" },
            new() { Account = "Assets", Amount = 1000m, Commodity = "$" }
        };

        _mockProcessRunner
            .GetBalances(Arg.Any<string>(), Arg.Any<string[]?>())
            .Returns(new HledgerBalanceResult
            {
                Balances = balances,
                TotalBalance = 1500m
            });

        var query = new GetBalanceQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Balances.Count.ShouldBe(2); // Cash, Assets (both root level)
        result.Balances.ShouldContain(b => b.Account == "Cash" && b.Depth == 1);
        result.Balances.ShouldContain(b => b.Account == "Assets" && b.Depth == 1);
    }

    [Fact]
    public async Task BuildHierarchy_EmptyBalanceList_ReturnsEmptyHierarchy()
    {
        // Arrange
        _mockProcessRunner
            .GetBalances(Arg.Any<string>(), Arg.Any<string[]?>())
            .Returns(new HledgerBalanceResult
            {
                Balances = new List<BalanceEntry>(),
                TotalBalance = 0m
            });

        var query = new GetBalanceQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Balances.ShouldBeEmpty();
    }

    [Fact]
    public async Task BuildHierarchy_DepthCalculation_IsCorrect()
    {
        // Arrange
        var balances = new List<BalanceEntry>
        {
            new() { Account = "Assets:Bank:Checking:Primary", Amount = 1000m, Commodity = "$" }
        };

        _mockProcessRunner
            .GetBalances(Arg.Any<string>(), Arg.Any<string[]?>())
            .Returns(new HledgerBalanceResult
            {
                Balances = balances,
                TotalBalance = 1000m
            });

        var query = new GetBalanceQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        // Root: Assets (depth 1, stub)
        var assets = result.Balances[0];
        assets.Depth.ShouldBe(1);

        // Level 2: Assets:Bank (depth 2, stub)
        var bank = assets.Children[0];
        bank.Depth.ShouldBe(2);

        // Level 3: Assets:Bank:Checking (depth 3, stub)
        var checking = bank.Children[0];
        checking.Depth.ShouldBe(3);

        // Level 4: Assets:Bank:Checking:Primary (depth 4, actual data)
        var primary = checking.Children[0];
        primary.Depth.ShouldBe(4);
        primary.Balance.ShouldBe(1000m);
    }

    [Fact]
    public async Task CalculateParentBalances_StubParents_SumsChildrenBalances()
    {
        // Arrange - hledger returns child accounts without parent
        var balances = new List<BalanceEntry>
        {
            new() { Account = "Assets:Checking", Amount = 1000m, Commodity = "$" },
            new() { Account = "Assets:Savings", Amount = 5000m, Commodity = "$" },
            new() { Account = "Expenses:Groceries", Amount = 250m, Commodity = "$" }
        };

        _mockProcessRunner
            .GetBalances(Arg.Any<string>(), Arg.Any<string[]?>())
            .Returns(new HledgerBalanceResult
            {
                Balances = balances,
                TotalBalance = 6250m
            });

        var query = new GetBalanceQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert - stub parent balances should be sum of children
        var assets = result.Balances.FirstOrDefault(b => b.Account == "Assets");
        assets.ShouldNotBeNull();
        assets.Balance.ShouldBe(6000m); // 1000 + 5000 (sum of children)
        assets.Children.Count.ShouldBe(2);

        var expenses = result.Balances.FirstOrDefault(b => b.Account == "Expenses");
        expenses.ShouldNotBeNull();
        expenses.Balance.ShouldBe(250m); // Sum of Groceries child
        expenses.Children.Count.ShouldBe(1);
    }

    [Fact]
    public async Task CalculateParentBalances_MultiLevelStubs_SumsRecursively()
    {
        // Arrange - deep hierarchy with missing intermediate parents
        var balances = new List<BalanceEntry>
        {
            new() { Account = "Assets:Bank:Checking:Primary", Amount = 1000m, Commodity = "$" },
            new() { Account = "Assets:Bank:Checking:Secondary", Amount = 500m, Commodity = "$" },
            new() { Account = "Assets:Bank:Savings", Amount = 3000m, Commodity = "$" }
        };

        _mockProcessRunner
            .GetBalances(Arg.Any<string>(), Arg.Any<string[]?>())
            .Returns(new HledgerBalanceResult
            {
                Balances = balances,
                TotalBalance = 4500m
            });

        var query = new GetBalanceQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert - balances should propagate up the hierarchy
        var assets = result.Balances[0];
        assets.Account.ShouldBe("Assets");
        assets.Balance.ShouldBe(4500m); // Sum of all descendants

        var bank = assets.Children[0];
        bank.Account.ShouldBe("Assets:Bank");
        bank.Balance.ShouldBe(4500m); // Sum of Checking + Savings

        var checking = bank.Children.FirstOrDefault(c => c.Account == "Assets:Bank:Checking");
        checking.ShouldNotBeNull();
        checking.Balance.ShouldBe(1500m); // 1000 + 500 (Primary + Secondary)

        var savings = bank.Children.FirstOrDefault(c => c.Account == "Assets:Bank:Savings");
        savings.ShouldNotBeNull();
        savings.Balance.ShouldBe(3000m); // Leaf node, original balance
    }
}

