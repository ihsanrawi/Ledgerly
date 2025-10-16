using FluentValidation;
using Ledgerly.Contracts.Dtos;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Validator for column mapping detection results.
/// Ensures required fields (date, amount) are detected with sufficient confidence.
/// </summary>
public class ColumnMappingValidator : AbstractValidator<ColumnDetectionResult>
{
    private const decimal MinimumConfidenceThreshold = 0.7m;

    public ColumnMappingValidator()
    {
        // Date column is required
        RuleFor(x => x.ConfidenceScores)
            .Must(scores => scores.ContainsKey("date") && scores["date"] >= MinimumConfidenceThreshold)
            .WithMessage($"Date column must be detected with confidence >= {MinimumConfidenceThreshold}");

        // Amount column OR (debit AND credit) columns are required
        RuleFor(x => x.ConfidenceScores)
            .Must(scores =>
            {
                var hasAmount = scores.ContainsKey("amount") && scores["amount"] >= MinimumConfidenceThreshold;
                var hasDebitCredit = scores.ContainsKey("debit") && scores.ContainsKey("credit");
                return hasAmount || hasDebitCredit;
            })
            .WithMessage($"Amount column (or Debit/Credit columns) must be detected with confidence >= {MinimumConfidenceThreshold}");

        // AllRequiredFieldsDetected must be true
        RuleFor(x => x.AllRequiredFieldsDetected)
            .Equal(true)
            .WithMessage("All required fields (date and amount) must be detected");
    }
}
