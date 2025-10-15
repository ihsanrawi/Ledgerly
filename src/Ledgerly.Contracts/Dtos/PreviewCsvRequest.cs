namespace Ledgerly.Contracts.Dtos;

/// <summary>
/// Request DTO for CSV file upload and preview.
/// Note: IFormFile is used directly in endpoint, not in this contract DTO.
/// This exists for future use when wrapping file upload in a request object.
/// </summary>
public record PreviewCsvRequest
{
    public string FileName { get; init; } = string.Empty;
    public long FileSize { get; init; }
}
