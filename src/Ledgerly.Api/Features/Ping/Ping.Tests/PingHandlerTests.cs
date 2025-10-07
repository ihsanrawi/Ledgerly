using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Wolverine;
using Xunit;

namespace Ledgerly.Api.Features.Ping;

/// <summary>
/// Wolverine Test Harness Pattern Demonstration
///
/// This test demonstrates how to test Wolverine command handlers using the in-memory message bus.
///
/// Key Concepts:
/// 1. Use Host.CreateDefaultBuilder() with UseWolverine() to create test host
/// 2. Configure handler discovery via opts.Discovery.IncludeAssembly()
/// 3. Get IMessageBus from DI container to access message bus
/// 4. Use InvokeAsync() to send commands and receive results
/// 5. Dispose host after test completes (using pattern)
///
/// This pattern allows testing handler logic without spinning up the full ASP.NET pipeline.
/// </summary>
public class PingHandlerTests : IAsyncLifetime
{
    private IHost? _host;
    private IMessageBus? _messageBus;

    public async Task InitializeAsync()
    {
        _host = await Host.CreateDefaultBuilder()
            .UseWolverine(opts =>
            {
                // Include assembly containing handlers for discovery
                opts.Discovery.IncludeAssembly(typeof(PingHandler).Assembly);
            })
            .StartAsync();

        _messageBus = _host.Services.GetRequiredService<IMessageBus>();
    }

    public async Task DisposeAsync()
    {
        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }

    [Fact]
    public async Task PingHandler_ShouldReturnPong_WhenPingCommandSent()
    {
        // Act: Send PingCommand via in-memory message bus
        var result = await _messageBus!.InvokeAsync<string>(new PingCommand());

        // Assert: Verify handler returned expected result
        result.ShouldBe("Pong");
    }

    [Fact]
    public async Task PingHandler_ShouldExecuteWithoutExternalDependencies()
    {
        // Act: Send command
        var result = await _messageBus!.InvokeAsync<string>(new PingCommand());

        // Assert: Verify handler executed successfully
        result.ShouldNotBeNull();
        result.ShouldBe("Pong");
    }
}
