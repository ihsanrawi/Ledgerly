using FluentValidation;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// FluentValidation validator for SaveColumnMappingCommand.
/// Story 2.4 - Manual Column Mapping Interface.
/// Critical security: Prevents XSS and CSV injection via bank identifier.
/// </summary>
public class SaveColumnMappingValidator : AbstractValidator<SaveColumnMappingCommand>
{
    private static readonly string[] ValidFieldTypes = new[]
    {
        "date", "amount", "description", "memo", "balance", "account", "debit", "credit"
    };

    public SaveColumnMappingValidator()
    {
        // Bank identifier validation (SEC-001 mitigation)
        RuleFor(x => x.BankIdentifier)
            .NotEmpty()
            .WithMessage("Bank identifier is required")
            .MaximumLength(100)
            .WithMessage("Bank identifier must not exceed 100 characters")
            .Matches(@"^[a-zA-Z0-9\s\-_]+$")
            .WithMessage("Bank identifier can only contain letters, numbers, spaces, hyphens, and underscores");

        // Column mappings validation (DATA-001 mitigation)
        RuleFor(x => x.ColumnMappings)
            .NotEmpty()
            .WithMessage("Column mappings cannot be empty");

        // Required fields validation (DATA-001 mitigation)
        RuleFor(x => x.ColumnMappings)
            .Must(ContainRequiredFields)
            .WithMessage("Column mappings must include required fields: date and amount (or debit/credit)");

        // Field type validation
        RuleFor(x => x.ColumnMappings)
            .Must(HaveValidFieldTypes)
            .WithMessage($"Column mappings can only contain valid field types: {string.Join(", ", ValidFieldTypes)}");

        // Header signature validation (TECH-001 mitigation)
        RuleFor(x => x.HeaderSignature)
            .NotEmpty()
            .WithMessage("Header signature is required for bank matching");

        // File name pattern validation (optional)
        When(x => !string.IsNullOrEmpty(x.FileNamePattern), () =>
        {
            RuleFor(x => x.FileNamePattern)
                .MaximumLength(200)
                .WithMessage("File name pattern must not exceed 200 characters");
        });
    }

    private bool ContainRequiredFields(Dictionary<string, string> mappings)
    {
        var hasDate = mappings.Values.Any(v => v.Equals("date", StringComparison.OrdinalIgnoreCase));

        // Amount can be either a single "amount" field or split "debit"/"credit" fields
        var hasAmount = mappings.Values.Any(v => v.Equals("amount", StringComparison.OrdinalIgnoreCase));
        var hasDebitCredit = mappings.Values.Any(v => v.Equals("debit", StringComparison.OrdinalIgnoreCase)) ||
                             mappings.Values.Any(v => v.Equals("credit", StringComparison.OrdinalIgnoreCase));

        return hasDate && (hasAmount || hasDebitCredit);
    }

    private bool HaveValidFieldTypes(Dictionary<string, string> mappings)
    {
        return mappings.Values.All(v => ValidFieldTypes.Contains(v.ToLowerInvariant()));
    }
}
