using Wolverine;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// API endpoint for deleting saved column mapping rules.
/// Story 2.4 - Manual Column Mapping Interface (AC: 6).
/// </summary>
public static class DeleteColumnMappingEndpoint
{
    public static void MapDeleteColumnMappingEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/import/mappings/{id:guid}", async (Guid id, IMessageBus bus, CancellationToken ct) =>
        {
            await bus.InvokeAsync(new DeleteColumnMappingCommand { Id = id }, ct);
            return Results.Ok(new { message = "Mapping deleted successfully" });
        })
        .WithName("DeleteColumnMapping")
        .WithTags("Import")
        .Produces(200)
        .Produces(400)
        .Produces(404)
        .Produces(500);
    }
}
