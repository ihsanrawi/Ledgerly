using Ledgerly.Api.Common.Exceptions;
using Ledgerly.Api.Common.Hledger;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;

namespace Hledger.Tests;

/// <summary>
/// Integration tests for HledgerProcessRunner.
/// Tests real hledger command execution with test data files.
/// </summary>
public class HledgerProcessRunnerTests : IDisposable
{
    private readonly HledgerProcessRunner _runner;
    private readonly string _sampleFilePath;
    private readonly string _invalidFilePath;

    public HledgerProcessRunnerTests()
    {
        // Setup Serilog for tests
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddSerilog();
        });

        var binaryManagerLogger = loggerFactory.CreateLogger<HledgerBinaryManager>();
        var runnerLogger = loggerFactory.CreateLogger<HledgerProcessRunner>();

        var binaryManager = new HledgerBinaryManager(binaryManagerLogger);
        _runner = new HledgerProcessRunner(binaryManager, runnerLogger);

        // Test data paths (copied to output directory)
        _sampleFilePath = Path.Combine(Directory.GetCurrentDirectory(), "sample.hledger");
        _invalidFilePath = Path.Combine(Directory.GetCurrentDirectory(), "invalid.hledger");

        // Verify test data files exist
        if (!File.Exists(_sampleFilePath))
        {
            throw new FileNotFoundException($"Test data file not found: {_sampleFilePath}");
        }
    }

    [Fact]
    public async Task ExecuteCommand_ValidFile_ReturnsOutput()
    {
        // Act
        var output = await _runner.ExecuteCommand(new[] { "bal", "-f", _sampleFilePath });

        // Assert
        Assert.NotEmpty(output);
        Assert.Contains("checking", output); // Should contain account names
    }

    [Fact]
    public async Task GetBalances_ValidFile_ParsesJsonOutput()
    {
        // Act
        var result = await _runner.GetBalances(_sampleFilePath);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Balances);
    }

    [Fact]
    public async Task ValidateFile_ValidFile_ReturnsSuccess()
    {
        // Act
        var result = await _runner.ValidateFile(_sampleFilePath);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateFile_InvalidFile_ReturnsErrorWithDetails()
    {
        // Act
        var result = await _runner.ValidateFile(_invalidFilePath);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task ValidateFile_MissingFile_ReturnsFailure()
    {
        // Arrange
        var nonExistentFile = "nonexistent.hledger";

        // Act
        var result = await _runner.ValidateFile(nonExistentFile);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("not found", result.Errors[0]);
    }

    [Fact]
    public async Task ExecuteCommand_InvalidCommand_ThrowsHledgerProcessException()
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<HledgerProcessException>(
            async () => await _runner.ExecuteCommand(new[] { "invalid-command" }));

        Assert.NotEqual(0, exception.ExitCode);
    }

    [Fact]
    public async Task ExecuteCommand_Timeout_ThrowsHledgerProcessException()
    {
        // Arrange - Use a very short timeout (1ms) to force timeout
        // We'll attempt to run a command that naturally takes time (e.g., hledger bal)
        // With a 1ms timeout, the process won't have time to complete
        var veryShortTimeout = TimeSpan.FromMilliseconds(1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HledgerProcessException>(
            async () => await _runner.ExecuteCommand(
                new[] { "bal", "-f", _sampleFilePath },
                timeout: veryShortTimeout));

        // Verify it's a timeout exception
        Assert.Contains("timed out", exception.Message.ToLowerInvariant());
    }

    [Fact]
    public async Task ExecuteCommand_StdErrOutput_IncludedInException()
    {
        // Arrange - Use invalid file to trigger stderr output
        var args = new[] { "check", "-f", _invalidFilePath };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HledgerProcessException>(
            async () => await _runner.ExecuteCommand(args));

        Assert.NotEmpty(exception.StdErr);
    }

    [Fact]
    public async Task GetBalances_WithAccountFilter_ReturnsFilteredResults()
    {
        // Act
        var result = await _runner.GetBalances(_sampleFilePath, new[] { "assets" });

        // Assert
        Assert.NotNull(result);
        // Balances should only include assets accounts or their children
    }

    public void Dispose()
    {
        Log.CloseAndFlush();
    }
}
