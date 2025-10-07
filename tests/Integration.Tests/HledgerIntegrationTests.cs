using Ledgerly.Api.Common.Hledger;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Integration.Tests;

/// <summary>
/// Integration tests for hledger binary execution and file validation
/// </summary>
public class HledgerIntegrationTests : IntegrationTestBase
{
    private readonly ILogger<HledgerBinaryManager> _binaryLogger;
    private readonly ILogger<HledgerProcessRunner> _processLogger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HledgerIntegrationTests()
    {
        _binaryLogger = Substitute.For<ILogger<HledgerBinaryManager>>();
        _processLogger = Substitute.For<ILogger<HledgerProcessRunner>>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
    }

    [Fact]
    public async Task HledgerBinary_ShouldExecute_AndReturnVersion()
    {
        // Arrange
        var binaryManager = new HledgerBinaryManager(_binaryLogger, _httpContextAccessor);

        // Act
        var version = await binaryManager.GetHledgerVersion();

        // Assert
        version.ShouldNotBeNullOrEmpty();
        version.ShouldMatch(@"\d+\.\d+\.\d+"); // Matches version pattern like "1.32.3"
    }

    [Fact]
    public async Task HledgerCheck_ShouldSucceed_WithValidHledgerFile()
    {
        // Arrange
        var binaryManager = new HledgerBinaryManager(_binaryLogger, _httpContextAccessor);
        var processRunner = new HledgerProcessRunner(binaryManager, _processLogger, _httpContextAccessor);

        var testFilePath = Path.Combine(TestDataDirectory, "test.hledger");
        await File.WriteAllTextAsync(testFilePath, @"
2025-01-01 Opening Balance
    Assets:Checking       $1000.00
    Equity:Opening
");

        // Act
        var result = await processRunner.ValidateFile(testFilePath);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Fact]
    public async Task HledgerCheck_ShouldFail_WithInvalidHledgerFile()
    {
        // Arrange
        var binaryManager = new HledgerBinaryManager(_binaryLogger, _httpContextAccessor);
        var processRunner = new HledgerProcessRunner(binaryManager, _processLogger, _httpContextAccessor);

        var testFilePath = Path.Combine(TestDataDirectory, "invalid.hledger");
        await File.WriteAllTextAsync(testFilePath, @"
2025-01-01 Unbalanced Transaction
    Assets:Checking       $100.00
");

        // Act
        var result = await processRunner.ValidateFile(testFilePath);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task HledgerBalance_ShouldReturnBalances_WithParsedResults()
    {
        // Arrange
        var binaryManager = new HledgerBinaryManager(_binaryLogger, _httpContextAccessor);
        var processRunner = new HledgerProcessRunner(binaryManager, _processLogger, _httpContextAccessor);

        var testFilePath = Path.Combine(TestDataDirectory, "balances.hledger");
        await File.WriteAllTextAsync(testFilePath, @"
2025-01-01 Opening Balance
    Assets:Checking       $1000.00
    Equity:Opening

2025-01-02 Purchase
    Expenses:Groceries     $50.00
    Assets:Checking
");

        // Act
        var result = await processRunner.GetBalances(testFilePath);

        // Assert
        result.ShouldNotBeNull();
        result.Balances.ShouldNotBeEmpty();

        var checkingBalance = result.Balances.FirstOrDefault(b => b.Account == "Assets:Checking");
        checkingBalance.ShouldNotBeNull();
        checkingBalance.Amount.ShouldBe(950.00m); // 1000 - 50
    }
}
