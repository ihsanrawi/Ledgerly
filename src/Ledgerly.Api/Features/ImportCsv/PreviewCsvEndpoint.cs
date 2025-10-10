using Ledgerly.Contracts.Dtos;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// Wolverine HTTP endpoint for CSV file upload and preview.
/// Accepts multipart/form-data file upload and returns preview with format detection.
/// </summary>
public class PreviewCsvEndpoint
{
    /// <summary>
    /// Upload CSV file for preview and format detection.
    /// </summary>
    /// <param name="file">CSV file to upload</param>
    /// <param name="bus">Wolverine message bus (injected)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>CSV preview response with headers, sample rows, and format info</returns>
    [WolverinePost("/api/import/preview")]
    public async Task<PreviewCsvResponse> Post(
        [FromForm] IFormFile file,
        IMessageBus bus,
        CancellationToken ct)
    {
        var command = new PreviewCsvCommand(file);
        var result = await bus.InvokeAsync<PreviewCsvResponse>(command, ct);
        return result;
    }
}
