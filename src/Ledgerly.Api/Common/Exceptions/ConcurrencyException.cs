namespace Ledgerly.Api.Common.Exceptions;

/// <summary>
/// Exception thrown when optimistic concurrency check fails.
/// </summary>
public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message) : base(message)
    {
    }

    public ConcurrencyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
