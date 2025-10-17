using FluentValidation;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// FluentValidation rules for GetCategorySuggestionsQuery (Story 2.5).
/// Validates transaction list and payee fields required for category suggestion matching.
/// </summary>
public class GetCategorySuggestionsQueryValidator : AbstractValidator<GetCategorySuggestionsQuery>
{
    private const int MaxTransactionsPerBatch = 1000;
    private const int MaxPayeeLength = 500;

    public GetCategorySuggestionsQueryValidator()
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
                    .WithMessage("Transaction payee is required for category suggestions")
                    .WithErrorCode("PAYEE_REQUIRED");

                transaction.RuleFor(t => t.Payee)
                    .MaximumLength(MaxPayeeLength)
                    .WithMessage($"Payee length must not exceed {MaxPayeeLength} characters")
                    .WithErrorCode("PAYEE_TOO_LONG");

                transaction.RuleFor(t => t.RowIndex)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("Row index must be non-negative")
                    .WithErrorCode("ROW_INDEX_INVALID");
            });
    }
}
