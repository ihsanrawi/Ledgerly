namespace Ledgerly.Api.Features.Ping;

/// <summary>
/// Sample handler for testing Wolverine message bus
/// </summary>
public static class PingHandler
{
    /// <summary>
    /// Handles PingCommand and returns "Pong"
    /// </summary>
    public static string Handle(PingCommand command)
    {
        return "Pong";
    }
}
