using System.Net;
using System.Net.Http.Json;
using Ledgerly.Contracts.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;

namespace Integration.Tests;

/// <summary>
/// Integration tests for GetBalance endpoint.
/// Tests full stack: HTTP → Wolverine → Handler → HledgerProcessRunner → hledger binary.
/// </summary>
public class GetBalanceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GetBalanceIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetBalance_WithSeedData_ReturnsCorrectBalances()
    {
        // Act
        var response = await _client.GetAsync("/api/balance");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<BalanceResponse>();
        result.ShouldNotBeNull();
        result.Balances.ShouldNotBeEmpty();
        result.AsOfDate.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-10), DateTime.UtcNow.AddSeconds(10));

        // Verify seed.hledger expected balances
        // Expected hierarchy (with stub nodes for parents showing sum of children):
        // - Assets ($4675.00) -> Assets:Checking ($4675.00)
        // - Equity ($-2000.00) -> Equity:Opening ($-2000.00)
        // - Expenses ($325.00) -> Expenses:Groceries ($250.00), Expenses:Dining ($75.00)
        // - Income ($-3000.00) -> Income:Salary ($-3000.00)

        var assets = result.Balances.FirstOrDefault(b => b.Account == "Assets");
        assets.ShouldNotBeNull();
        assets.Balance.ShouldBe(4675.00m); // Sum of children
        assets.Children.ShouldContain(c => c.Account == "Assets:Checking" && c.Balance == 4675.00m);

        var expenses = result.Balances.FirstOrDefault(b => b.Account == "Expenses");
        expenses.ShouldNotBeNull();
        expenses.Balance.ShouldBe(325.00m); // 250 + 75
        expenses.Children.ShouldContain(c => c.Account == "Expenses:Groceries" && c.Balance == 250.00m);
        expenses.Children.ShouldContain(c => c.Account == "Expenses:Dining" && c.Balance == 75.00m);

        var income = result.Balances.FirstOrDefault(b => b.Account == "Income");
        income.ShouldNotBeNull();
        income.Balance.ShouldBe(-3000.00m); // Sum of children
        income.Children.ShouldContain(c => c.Account == "Income:Salary" && c.Balance == -3000.00m);
    }

    [Fact]
    public async Task GetBalance_WithAccountFilter_ReturnsFilteredBalances()
    {
        // Act
        var response = await _client.GetAsync("/api/balance?accounts=Expenses");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<BalanceResponse>();
        result.ShouldNotBeNull();
        result.Balances.ShouldNotBeEmpty();

        // When filtering by "Expenses", hledger should return only Expenses accounts
        var expenses = result.Balances.FirstOrDefault(b => b.Account == "Expenses");
        expenses.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetBalance_ReturnsValidJsonStructure()
    {
        // Act
        var response = await _client.GetAsync("/api/balance");

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

        var result = await response.Content.ReadFromJsonAsync<BalanceResponse>();
        result.ShouldNotBeNull();
        result.Balances.ShouldNotBeNull();
        result.AsOfDate.ShouldNotBe(default);

        // Verify hierarchical structure
        foreach (var balance in result.Balances)
        {
            balance.Account.ShouldNotBeNullOrEmpty();
            balance.Depth.ShouldBeGreaterThan(0);
            balance.Children.ShouldNotBeNull();
        }
    }
}
