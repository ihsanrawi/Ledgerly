using Wolverine;

namespace Ledgerly.Api.Features.ImportCsv;

/// <summary>
/// API endpoint for retrieving saved column mapping rules.
/// Story 2.4 - Manual Column Mapping Interface (AC: 6).
/// </summary>
public static class GetSavedMappingsEndpoint
{
    public static void MapGetSavedMappingsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/import/mappings", async (IMessageBus bus, CancellationToken ct) =>
        {
            var response = await bus.InvokeAsync<List<Ledgerly.Contracts.Dtos.SavedMappingDto>>(new GetSavedMappingsQuery(), ct);
            return Results.Ok(response);
        })
        .WithName("GetSavedMappings")
        .WithTags("Import")
        .Produces<List<Ledgerly.Contracts.Dtos.SavedMappingDto>>(200)
        .Produces(500);
    }
}
