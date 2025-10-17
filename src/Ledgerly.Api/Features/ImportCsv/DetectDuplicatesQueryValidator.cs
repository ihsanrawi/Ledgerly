using FluentValidation;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// FluentValidation rules for DetectDuplicatesQuery (Story 2.5).
/// Validates transaction list and individual transaction fields required for duplicate detection.
/// </summary>
public class DetectDuplicatesQueryValidator : AbstractValidator<DetectDuplicatesQuery>
{
    private const int MaxTransactionsPerBatch = 1000;
    private const int MaxPayeeLength = 500;

    public DetectDuplicatesQueryValidator()
    {
        RuleFor(x => x.Transactions)
            .NotNull()
            .WithMessage("Transactions list is required")
            .WithErrorCode("TRANSACTIONS_REQUIRED");

        RuleFor(x => x.Transactions)
            .Must(list => list.Count > 0)
            .When(x => x.Transactions != null)
            .WithMessage("At least one transaction is required")
            .WithErrorCode("TRANSACTIONS_EMPTY");

        RuleFor(x => x.Transactions)
            .Must(list => list.Count <= MaxTransactionsPerBatch)
            .When(x => x.Transactions != null)
            .WithMessage($"Cannot process more than {MaxTransactionsPerBatch} transactions in a single batch")
            .WithErrorCode("TRANSACTIONS_BATCH_TOO_LARGE");

        RuleForEach(x => x.Transactions)
            .ChildRules(transaction =>
            {
                transaction.RuleFor(t => t.Payee)
                    .NotEmpty()
                    .WithMessage("Transaction payee is required for duplicate detection")
                    .WithErrorCode("PAYEE_REQUIRED");

                transaction.RuleFor(t => t.Payee)
                    .MaximumLength(MaxPayeeLength)
                    .WithMessage($"Payee length must not exceed {MaxPayeeLength} characters")
                    .WithErrorCode("PAYEE_TOO_LONG");

                transaction.RuleFor(t => t.Date)
                    .NotEmpty()
                    .WithMessage("Transaction date is required for duplicate detection")
                    .WithErrorCode("DATE_REQUIRED");

                transaction.RuleFor(t => t.Date)
                    .Must(date => date >= new DateTime(1900, 1, 1) && date <= DateTime.UtcNow.AddYears(1))
                    .WithMessage("Transaction date must be between 1900-01-01 and one year in the future")
                    .WithErrorCode("DATE_OUT_OF_RANGE");

                transaction.RuleFor(t => t.Amount)
                    .Must(amount => amount != 0)
                    .WithMessage("Transaction amount cannot be zero")
                    .WithErrorCode("AMOUNT_ZERO");

                transaction.RuleFor(t => t.RowIndex)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Row index must be non-negative")
                    .WithErrorCode("ROW_INDEX_INVALID");
            });
    }
}
