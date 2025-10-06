namespace Ledgerly.Api.Common.Hledger;

/// <summary>
/// Result from hledger check validation.
/// </summary>
public class ValidationResult
{
    public required bool IsValid { get; init; }
    public required List<string> Errors { get; init; }

    public static ValidationResult Success() => new()
    {
        IsValid = true,
        Errors = new List<string>()
    };

    public static ValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = new List<string>(errors)
    };
}
