using Wolverine;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// API endpoint for saving column mapping rules.
/// Story 2.4 - Manual Column Mapping Interface (AC: 5, 6).
/// </summary>
public static class SaveColumnMappingEndpoint
{
    public static void MapSaveColumnMappingEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/import/save-mapping", async (SaveColumnMappingCommand command, IMessageBus bus, CancellationToken ct) =>
        {
            var response = await bus.InvokeAsync<Ledgerly.Contracts.Dtos.SaveColumnMappingResponse>(command, ct);
            return Results.Ok(response);
        })
        .WithName("SaveColumnMapping")
        .WithTags("Import")
        .Produces<Ledgerly.Contracts.Dtos.SaveColumnMappingResponse>(200)
        .Produces(400)
        .Produces(500);
    }
}
