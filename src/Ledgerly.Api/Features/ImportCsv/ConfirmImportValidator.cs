using FluentValidation;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Validator for ConfirmImportCommand.
/// Ensures all required fields are present and valid before persisting transactions.
/// Story 2.6: Import Preview and Confirmation
/// </summary>
public class ConfirmImportValidator : AbstractValidator<ConfirmImportCommand>
{
    public ConfirmImportValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty()
            .WithMessage("File name is required")
            .MaximumLength(255)
            .WithMessage("File name must not exceed 255 characters");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.Transactions)
            .NotEmpty()
            .WithMessage("At least one transaction is required for import");

        RuleForEach(x => x.Transactions)
            .ChildRules(transaction =>
            {
                transaction.RuleFor(t => t.Date)
                    .NotEmpty()
                    .WithMessage("Transaction date is required")
                    .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1))
                    .WithMessage("Transaction date cannot be in the future");

                transaction.RuleFor(t => t.Payee)
                    .NotEmpty()
                    .WithMessage("Payee is required")
                    .MaximumLength(200)
                    .WithMessage("Payee must not exceed 200 characters");

                transaction.RuleFor(t => t.Amount)
                    .NotEqual(0)
                    .WithMessage("Amount cannot be zero")
                    .Must(amount => amount > -1_000_000_000 && amount < 1_000_000_000)
                    .WithMessage("Amount must be within valid range");

                transaction.RuleFor(t => t.Category)
                    .NotEmpty()
                    .WithMessage("Category is required")
                    .MaximumLength(200)
                    .WithMessage("Category must not exceed 200 characters");

                transaction.RuleFor(t => t.Account)
                    .NotEmpty()
                    .WithMessage("Account is required")
                    .MaximumLength(200)
                    .WithMessage("Account must not exceed 200 characters");

                transaction.RuleFor(t => t.Memo)
                    .MaximumLength(500)
                    .When(t => !string.IsNullOrEmpty(t.Memo))
                    .WithMessage("Memo must not exceed 500 characters");

                transaction.RuleFor(t => t.IsDuplicate)
                    .Equal(false)
                    .WithMessage("Duplicate transactions should be filtered out before confirmation");
            });
    }
}
