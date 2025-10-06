using Ledgerly.Api.Common.Exceptions;
using Ledgerly.Api.Common.Hledger;
using Microsoft.Extensions.Logging;
using Serilog;
using Xunit;

namespace Hledger.Tests;

/// <summary>
/// Integration tests for HledgerBinaryManager.
/// Tests real binary extraction, SHA256 verification, and version execution.
/// </summary>
public class HledgerBinaryManagerTests : IDisposable
{
    private readonly HledgerBinaryManager _manager;
    private readonly string _testExtractionPath;

    public HledgerBinaryManagerTests()
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

        var logger = loggerFactory.CreateLogger<HledgerBinaryManager>();
        _manager = new HledgerBinaryManager(logger);

        // Store extraction path for cleanup
        _testExtractionPath = GetExtractionPath();
    }

    [Fact]
    public async Task GetHledgerBinaryPath_FirstRun_ExtractsBinarySuccessfully()
    {
        // Arrange - Clean up any existing binary
        CleanupExtractedBinary();

        // Act
        var binaryPath = await _manager.GetHledgerBinaryPath();

        // Assert
        Assert.False(string.IsNullOrEmpty(binaryPath));
        Assert.True(File.Exists(binaryPath), $"Binary should exist at {binaryPath}");
    }

    [Fact]
    public async Task ValidateBinary_ValidBinary_PassesSha256Verification()
    {
        // Arrange - Ensure binary is extracted
        await _manager.GetHledgerBinaryPath();

        // Act
        var isValid = await _manager.ValidateBinary();

        // Assert
        Assert.True(isValid, "SHA256 verification should pass for valid binary");
    }

    [Fact]
    public async Task GetHledgerVersion_ValidBinary_ReturnsExpectedVersion()
    {
        // Arrange
        await _manager.GetHledgerBinaryPath();

        // Act
        var version = await _manager.GetHledgerVersion();

        // Assert
        Assert.Equal("1.32.3", version);
    }

    [Fact]
    public async Task GetHledgerBinaryPath_MultipleInvocations_ReturnsCachedPath()
    {
        // Arrange
        CleanupExtractedBinary();

        // Act
        var firstPath = await _manager.GetHledgerBinaryPath();
        var secondPath = await _manager.GetHledgerBinaryPath();

        // Assert
        Assert.Equal(firstPath, secondPath);
    }

    [Fact]
    public async Task ExtractEmbeddedBinary_ValidResource_SetsExecutablePermissions()
    {
        // Arrange
        CleanupExtractedBinary();

        // Act
        await _manager.ExtractEmbeddedBinary();
        var binaryPath = await _manager.GetHledgerBinaryPath();

        // Assert - Try executing --version
        var version = await _manager.GetHledgerVersion();
        Assert.NotEmpty(version);
    }

    [Fact]
    public async Task ValidateBinary_MissingBinary_ReturnsFalse()
    {
        // Arrange
        CleanupExtractedBinary();

        // Act
        var isValid = await _manager.ValidateBinary();

        // Assert
        Assert.False(isValid, "Validation should fail when binary is missing");
    }

    public void Dispose()
    {
        // Cleanup is optional - extracted binaries can be reused across test runs
        // Uncomment to clean up after each test:
        // CleanupExtractedBinary();
        Log.CloseAndFlush();
    }

    private void CleanupExtractedBinary()
    {
        if (Directory.Exists(_testExtractionPath))
        {
            try
            {
                Directory.Delete(_testExtractionPath, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    private static string GetExtractionPath()
    {
        string basePath;

        if (OperatingSystem.IsWindows())
        {
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        }
        else
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            basePath = Path.Combine(home, ".local", "share");
        }

        return Path.Combine(basePath, "Ledgerly", "bin");
    }
}
