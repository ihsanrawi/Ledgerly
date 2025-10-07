using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Integration.Tests;

/// <summary>
/// Base class for integration tests providing common test infrastructure
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected string TestDataDirectory { get; private set; } = string.Empty;

    public Task InitializeAsync()
    {
        // Create temp directory for .hledger files
        TestDataDirectory = Path.Combine(Path.GetTempPath(), $"ledgerly-tests-{Guid.NewGuid()}");
        Directory.CreateDirectory(TestDataDirectory);

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        // Cleanup temp directory
        if (Directory.Exists(TestDataDirectory))
        {
            Directory.Delete(TestDataDirectory, recursive: true);
        }

        return Task.CompletedTask;
    }
}
