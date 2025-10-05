# Error Handling Strategy

## General Approach

**Error Model:** Typed exceptions with structured error responses

**Exception Hierarchy:**
```
LedgerlyException (base)
├── ValidationException (FluentValidation failures)
├── HledgerException (base for hledger-related errors)
│   ├── HledgerValidationException (hledger check failures)
│   ├── HledgerBinaryNotFoundException
│   ├── HledgerProcessException (CLI execution failures)
│   └── HledgerParseException (output parsing failures)
├── CacheException (base for cache-related errors)
│   ├── CacheSyncException (FileSystemWatcher/rebuild failures)
│   └── CacheOutOfSyncException (hash mismatch detected)
├── ConcurrencyException (optimistic locking failures)
├── DuplicateTransactionException (CSV import duplicates)
└── FileAccessException (permission denied, file locked)
```

**Error Propagation:**
- **API Layer:** Catch all exceptions, map to HTTP status codes + structured JSON
- **Application Layer (Handlers):** Throw domain exceptions, no HTTP awareness
- **Domain Layer (Shared Kernel):** Throw specific exceptions with context
- **Database Layer (EF Core):** `DbUpdateConcurrencyException` → `ConcurrencyException`

**Error Response Format:**
```json
{
  "errorCode": "HLEDGER_VALIDATION_FAILED",
  "message": "Transaction validation failed",
  "details": "could not balance this transaction...",
  "validationErrors": null,
  "timestamp": "2025-01-15T14:30:00Z",
  "traceId": "abc-123-def-456"
}
```

## Logging Standards

**Library:** Serilog 3.1.1 with file sink

**Format:** Structured JSON logging

**Log Levels:**
- **Debug:** Detailed diagnostic (dev only)
- **Information:** General flow (startup, hledger execution)
- **Warning:** Unexpected but recoverable (cache out of sync, slow query >2s)
- **Error:** Operation failures requiring attention (import failed)
- **Fatal:** Application crashes (unhandled exceptions, DB corruption)

**Required Context:**
- **Correlation ID:** GUID per API request
- **Service Context:** Feature slice name (e.g., "ImportCsv")
- **User Context:** No PII - anonymized machine ID hash only

**Location:** `%APPDATA%/Ledgerly/logs/ledgerly-{Date}.log`
**Retention:** Last 7 days (auto-delete)

**Log Sanitization:**
- Never log: Passwords, API keys, full file paths
- Redact: Payee names in production (unless debug)
- Hash: User identifiers (machine ID → SHA256)

## Error Handling Patterns

### External API Errors (hledger CLI)

**Retry Policy:**
- Transient errors (timeout): Exponential backoff, max 3 retries
- Permanent errors (invalid syntax): Fail immediately

**Timeout Configuration:**
- Default: 30 seconds
- Large files (>50K transactions): 60 seconds

**Error Translation:**
```csharp
try
{
    var result = await _processRunner.ExecuteCommand("hledger", ["check", filePath]);
}
catch (ProcessExecutionException ex) when (ex.ExitCode == 1)
{
    var errorMessage = ParseHledgerError(ex.StdErr);
    throw new HledgerValidationException(errorMessage, ex);
}
```

**hledger Exit Code Mapping:**
| Exit Code | Meaning | Action |
|-----------|---------|--------|
| 0 | Success | Continue |
| 1 | Validation error | Show hledger error to user |
| 2 | Parse error | Show "Invalid .hledger syntax" |
| 127 | Binary not found | Offer to re-extract |

### Business Logic Errors

**Custom Exceptions:**
```csharp
public class DuplicateTransactionException : LedgerlyException
{
    public string TransactionHash { get; }
    public DateTime OriginalDate { get; }

    public DuplicateTransactionException(string hash, DateTime date, string payee)
        : base($"Duplicate transaction: {payee} on {date:yyyy-MM-dd}")
    {
        TransactionHash = hash;
        OriginalDate = date;
    }
}
```

**User-Facing Errors:**
- Plain English, actionable suggestions
- Example: "This transaction already exists. Imported on 2025-01-10. Skip or edit amount?"

**Error Codes:**
```csharp
public enum ErrorCode
{
    VALIDATION_FAILED,
    DUPLICATE_TRANSACTION,
    HLEDGER_VALIDATION_FAILED,
    HLEDGER_BINARY_NOT_FOUND,
    CACHE_OUT_OF_SYNC,
    CONCURRENCY_CONFLICT,
    FILE_ACCESS_DENIED,
    UNKNOWN_ERROR
}
```

### Data Consistency

**Transaction Strategy:**
```csharp
using var transaction = await _dbContext.Database.BeginTransactionAsync();
try
{
    // 1. Write .hledger (atomic temp → rename)
    await _hledgerWriter.AppendTransaction(transaction, filePath);

    // 2. Insert into SQLite cache
    _dbContext.Transactions.Add(transaction);
    await _dbContext.SaveChangesAsync();

    // 3. Update cache metadata
    await _cacheMetadata.UpdateHash(newFileHash);

    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    await _hledgerWriter.RestoreFromBackup(filePath);
    throw;
}
```

**Compensation Logic:**
- Failed write: Restore `.hledger.bak`
- Cache desync: Trigger full rebuild from .hledger
- Concurrency conflict: Prompt refresh

**Idempotency:**
- Import CSV: Check `FileHash` to prevent re-import
- Transaction create: Check `Hash` (date+payee+amount)
- Cache rebuild: Safe to run multiple times

## Frontend Error Handling

**Angular HTTP Interceptor:**
```typescript
intercept(req: HttpRequest<any>, next: HttpHandler) {
  return next.handle(req).pipe(
    catchError((error: HttpErrorResponse) => {
      const errorResponse = error.error as ApiErrorResponse;
      const userMessage = this.getUserMessage(errorResponse.errorCode);

      if (error.status >= 500) {
        this.toastr.error(userMessage, 'Server Error');
      } else if (error.status === 409) {
        this.toastr.warning(userMessage, 'Conflict');
      }

      console.error('API Error:', errorResponse);
      return throwError(() => errorResponse);
    })
  );
}
```

**User-Friendly Messages:**
```typescript
const messages: Record<string, string> = {
  'HLEDGER_VALIDATION_FAILED': 'Transaction is unbalanced. Please check your entry.',
  'DUPLICATE_TRANSACTION': 'This transaction already exists in your ledger.',
  'CONCURRENCY_CONFLICT': 'Someone else modified this record. Please refresh.',
  'HLEDGER_BINARY_NOT_FOUND': 'hledger engine not found. Please restart the app.',
};
```

## Critical Error Recovery

**Application Crash Recovery:**
```csharp
try
{
    var app = BuildApplication();
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");

    // Create crash report (user can manually send)
    var crashReport = new { Timestamp, Exception, OSVersion, HledgerVersion };
    File.WriteAllText(Path.Combine(AppData, "crash-report.json"),
                      JsonSerializer.Serialize(crashReport));

    ShowCrashDialog(ex.Message);
}
finally
{
    Log.CloseAndFlush();
}
```

**Database Corruption Recovery:**
```csharp
if (!await ValidateDatabaseIntegrity())
{
    Log.Warning("Database integrity failed. Rebuilding from .hledger.");

    // Backup corrupted DB
    File.Copy(dbPath, $"{dbPath}.corrupted-{DateTime.UtcNow:yyyyMMddHHmmss}.bak");

    // Delete and rebuild
    File.Delete(dbPath);
    await RebuildCacheFromHledgerFile();
}
```

---
