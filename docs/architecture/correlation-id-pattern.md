# Correlation ID Pattern

**Purpose:** Implement distributed tracing using a single correlation ID throughout the entire request lifecycle.

**Last Updated:** 2025-10-15

---

## Pattern Overview

This codebase uses **Serilog's LogContext enrichment** (Option 3) to automatically propagate correlation IDs through the entire request chain without manual parameter passing.

## Implementation

### 1. Middleware (Entry Point)

**File:** [Common/Middleware/CorrelationIdMiddleware.cs](../../src/Ledgerly.Api/Common/Middleware/CorrelationIdMiddleware.cs)

```csharp
public async Task InvokeAsync(HttpContext context)
{
    // Get correlation ID from header or generate new one
    var headerValue = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
    var correlationId = string.IsNullOrWhiteSpace(headerValue)
        ? Guid.NewGuid().ToString()
        : headerValue;

    // Add to response headers (for client tracing)
    context.Response.Headers.TryAdd("X-Correlation-ID", correlationId);

    // Push to Serilog LogContext - THIS IS THE KEY!
    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        // All logs within this scope automatically include CorrelationId
        await _next(context);
    }
}
```

### Correlation ID Fallback Logic:

**Scenario 1: Client provides valid ID**
```
Request Header: X-Correlation-ID: "abc-123"
→ Use "abc-123"
```

**Scenario 2: No header present (most common)**
```
Request Header: (none)
→ Generate new Guid
```

**Scenario 3: Empty/whitespace header**
```
Request Header: X-Correlation-ID: ""
Request Header: X-Correlation-ID: "   "
→ Generate new Guid (treats as missing)
```

**Why this matters:**
- Prevents empty correlation IDs in logs
- Ensures every request has a valid tracing ID
- Handles malformed client headers gracefully

### 2. Handler/Service Logging

**File:** [Features/ImportCsv/PreviewCsvHandler.cs](../../src/Ledgerly.Api/Features/ImportCsv/PreviewCsvHandler.cs)

```csharp
public async Task<PreviewCsvResponse> Handle(PreviewCsvCommand command, CancellationToken ct)
{
    // NO manual correlation ID generation!
    _logger.LogInformation(
        "Processing PreviewCsvCommand. FileName: {FileName}, FileSize: {FileSize}",
        command.File.FileName, command.File.Length);
    // CorrelationId is automatically included by Serilog

    // ... rest of handler logic
}
```

**File:** [Features/ImportCsv/ColumnDetectionService.cs](../../src/Ledgerly.Api/Features/ImportCsv/ColumnDetectionService.cs)

```csharp
public async Task<ColumnDetectionResult> DetectColumns(string[] headers, List<Dictionary<string, string>> sampleRows)
{
    // NO manual correlation ID generation or parameter passing!
    _logger.LogInformation(
        "Starting column detection. HeaderCount: {HeaderCount}, SampleRowCount: {SampleRowCount}",
        headers.Length, sampleRows.Count);
    // CorrelationId is automatically included by Serilog

    // ... rest of service logic
}
```

### 3. Log Output Example

All logs automatically include the CorrelationId:

```json
{
  "Timestamp": "2025-10-15T21:30:45.123Z",
  "Level": "Information",
  "MessageTemplate": "Processing PreviewCsvCommand. FileName: {FileName}, FileSize: {FileSize}",
  "Properties": {
    "FileName": "transactions.csv",
    "FileSize": 1024,
    "CorrelationId": "abc-123-def-456" // Automatically added!
  }
}

{
  "Timestamp": "2025-10-15T21:30:45.234Z",
  "Level": "Information",
  "MessageTemplate": "Starting column detection. HeaderCount: {HeaderCount}, SampleRowCount: {SampleRowCount}",
  "Properties": {
    "HeaderCount": 5,
    "SampleRowCount": 10,
    "CorrelationId": "abc-123-def-456" // Same ID throughout the chain!
  }
}

{
  "Timestamp": "2025-10-15T21:30:45.567Z",
  "Level": "Information",
  "MessageTemplate": "CSV preview completed. Duration: {Duration}ms, Rows: {RowCount}",
  "Properties": {
    "Duration": 444,
    "RowCount": 100,
    "CorrelationId": "abc-123-def-456" // Same ID!
  }
}
```

## Benefits

✅ **Single Source of Truth**: One correlation ID per HTTP request
✅ **No Manual Propagation**: Serilog LogContext handles it automatically
✅ **Clean Code**: No extra parameters cluttering method signatures
✅ **End-to-End Tracing**: Trace from HTTP request → Handler → Services → Database
✅ **Client Integration**: Response header allows client-side correlation
✅ **Log Aggregation**: Query logs by CorrelationId to see entire request flow

## Best Practices

### ✅ DO:
- Let middleware generate/extract correlation IDs
- Use `_logger.Log*()` methods without explicit correlation ID parameters
- Trust that Serilog's LogContext enrichment adds it automatically
- Include meaningful structured properties in logs (FileName, Duration, etc.)

### ❌ DON'T:
- Generate new `Guid.NewGuid()` correlation IDs in handlers or services
- Pass correlation IDs as method parameters (unless crossing process boundaries)
- Log raw correlation IDs in error messages to users
- Mix correlation IDs with other tracing mechanisms

## When to Use Manual Correlation IDs

Only pass correlation IDs explicitly when:
- **Calling external APIs** (include in HTTP headers)
- **Publishing to message queues** (include in message metadata)
- **Writing to separate processes** (not within the same ASP.NET request context)

For synchronous calls within the same HTTP request context, always rely on LogContext enrichment.

## Debugging with Correlation IDs

### Query logs by correlation ID:
```bash
# Using structured logging query
grep "abc-123-def-456" logs/ledgerly-*.log

# Using log aggregation tools
Seq: CorrelationId = "abc-123-def-456"
Splunk: index=ledgerly CorrelationId="abc-123-def-456"
```

### Trace request flow:
1. User uploads CSV file
2. Client receives `X-Correlation-ID` header in response
3. User reports error: "My upload failed at 2:30pm"
4. Search logs for correlation ID to see entire chain:
   - Request received
   - CSV parsing started
   - Column detection started
   - Error occurred: "Invalid date format on line 42"

---

## Migration Notes

**Before (Manual IDs):**
```csharp
var correlationId = Guid.NewGuid();
_logger.LogInformation("Processing request. CorrelationId: {CorrelationId}", correlationId);
```

**After (LogContext Enrichment):**
```csharp
// No manual generation!
_logger.LogInformation("Processing request");
// CorrelationId automatically included
```

**Changed in Story 2.3:**
- Removed manual correlation ID generation from `PreviewCsvHandler`
- Removed manual correlation ID generation from `ColumnDetectionService`
- Removed manual correlation ID generation from `CsvParserService`
- Removed manual correlation ID generation from `GetBalanceHandler`
- All correlation IDs now flow through `CorrelationIdMiddleware` + Serilog LogContext
- **Result**: Complete codebase now uses unified correlation ID pattern across all handlers and services

---

## References

- [Serilog LogContext Documentation](https://github.com/serilog/serilog/wiki/Enrichment#logcontext)
- [Middleware Implementation](../../src/Ledgerly.Api/Common/Middleware/CorrelationIdMiddleware.cs)
- [Program.cs Serilog Configuration](../../src/Ledgerly.Api/Program.cs)
