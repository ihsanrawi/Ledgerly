using System.Reflection;
using Ledgerly.Api.Common.Data;
using Ledgerly.Api.Common.Middleware;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Wolverine;
using Wolverine.Http;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Ledgerly")
    .WriteTo.Console()
    .WriteTo.File("logs/ledgerly-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting Ledgerly API");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog();

    // Add Wolverine
    builder.Host.UseWolverine(opts =>
    {
        // Configure local message bus
        opts.Policies.AutoApplyTransactions();

        // Discover HTTP endpoints in Features
        opts.Discovery.IncludeAssembly(Assembly.GetExecutingAssembly());
    });

    // Add Wolverine HTTP support
    builder.Services.AddWolverineHttp();

    // Add OpenAPI/Swagger
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Ledgerly API",
            Version = "v1",
            Description = "API for Ledgerly - Local-First Personal Finance with Plain Text Accounting"
        });

        // Include XML comments
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    });

    // Add CORS for development (restrict to localhost origins)
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(
                      "http://localhost:4200",  // Angular dev server
                      "http://localhost:5173",  // Vite alternative
                      "tauri://localhost",      // Tauri app
                      "https://tauri.localhost" // Tauri secure
                  )
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Add correlation ID middleware
    builder.Services.AddHttpContextAccessor();

    // Register hledger services
    builder.Services.AddSingleton<Ledgerly.Api.Common.Hledger.HledgerBinaryManager>();
    builder.Services.AddScoped<Ledgerly.Api.Common.Hledger.IHledgerProcessRunner, Ledgerly.Api.Common.Hledger.HledgerProcessRunner>();

    // Register CSV import services
    builder.Services.AddScoped<Ledgerly.Api.Features.ImportCsv.ICsvParserService, Ledgerly.Api.Features.ImportCsv.CsvParserService>();

    // Configure SQLite for caching only
    builder.Services.AddDbContext<LedgerlyDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("LedgerlyCache")
            ?? "Data Source=ledgerly-cache.db";
        options.UseSqlite(connectionString);
    });

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ledgerly API v1");
            c.RoutePrefix = "swagger";
        });
    }

    // Global exception handling (first in pipeline)
    app.UseMiddleware<GlobalExceptionMiddleware>();

    app.UseCors();

    // Add correlation ID middleware (before request logging)
    app.UseMiddleware<CorrelationIdMiddleware>();

    app.UseSerilogRequestLogging();

    // Wolverine HTTP endpoints
    app.MapWolverineEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Make Program class accessible to integration tests
public partial class Program { }
