namespace Ledgerly.Api.Common.Exceptions;

/// <summary>
/// Exception thrown when hledger validation fails.
/// </summary>
public class HledgerValidationException : Exception
{
    public HledgerValidationException(string message) : base(message)
    {
    }

    public HledgerValidationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
