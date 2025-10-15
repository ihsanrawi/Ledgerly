namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine command for CSV file upload and parsing preview.
/// </summary>
public record PreviewCsvCommand(IFormFile File);
