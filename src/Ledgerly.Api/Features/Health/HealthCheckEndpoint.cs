using Wolverine.Http;

namespace Ledgerly.Api.Features.Health;

/// <summary>
/// Health check endpoint for monitoring API availability.
/// </summary>
public class HealthCheckEndpoint
{
    /// <summary>
    /// Checks if the API is healthy and running.
    /// </summary>
    /// <returns>Health status response.</returns>
    [WolverineGet("/api/health")]
    public HealthResponse Get()
    {
        return new HealthResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Service = "Ledgerly API"
        };
    }
}

/// <summary>
/// Response model for health check.
/// </summary>
public record HealthResponse
{
    /// <summary>
    /// Health status (e.g., "Healthy", "Unhealthy").
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// Timestamp of the health check.
    /// </summary>
    public DateTime Timestamp { get; init; }

    /// <summary>
    /// Service name.
    /// </summary>
    public required string Service { get; init; }
}
