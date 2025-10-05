using System.Reflection;
using Ledgerly.Api.Common.Data;
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

    // Add CORS for development
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Add correlation ID middleware
    builder.Services.AddHttpContextAccessor();

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

    app.UseCors();
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
