using FluentValidation;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// FluentValidation rules for PreviewCsvCommand.
/// Validates file size (max 50MB) and file extension (.csv only).
/// </summary>
public class PreviewCsvValidator : AbstractValidator<PreviewCsvCommand>
{
    private const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50MB
    private static readonly string[] AllowedExtensions = { ".csv" };

    public PreviewCsvValidator()
    {
        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File is required")
            .WithErrorCode("FILE_REQUIRED");

        RuleFor(x => x.File.Length)
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage($"File size must not exceed {MaxFileSizeBytes / (1024 * 1024)}MB")
            .WithErrorCode("FILE_TOO_LARGE");

        RuleFor(x => x.File.FileName)
            .Must(HaveValidExtension)
            .WithMessage("Only .csv files are allowed")
            .WithErrorCode("INVALID_FILE_TYPE");

        RuleFor(x => x.File.ContentType)
            .Must(HaveValidContentType)
            .WithMessage("Invalid content type. Expected text/csv or application/vnd.ms-excel")
            .WithErrorCode("INVALID_CONTENT_TYPE");
    }

    private bool HaveValidExtension(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedExtensions.Contains(extension);
    }

    private bool HaveValidContentType(string contentType)
    {
        var validTypes = new[] { "text/csv", "application/vnd.ms-excel", "application/csv", "text/plain" };
        return validTypes.Contains(contentType.ToLowerInvariant());
    }
}
