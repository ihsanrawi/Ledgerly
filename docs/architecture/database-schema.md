# Database Schema

**Purpose:** Query performance cache for hledger data (SQLite 3.45.1 with EF Core 8.0.4, SQLCipher 4.5.6 for encryption)

**CRITICAL:** This database is a **CACHE LAYER ONLY**. All authoritative financial data resides in .hledger files.

## Schema Design (v2.0 - with precision and concurrency fixes)

**Key Changes from v1.0:**
1. **Amount/Balance:** Changed from REAL to INTEGER (stores cents for exact precision)
2. **RowVersion:** Added for optimistic concurrency control
3. **CategoryId FK:** Added while keeping Category TEXT for dual-tracking
4. **CurrencyCode:** Added for future multi-currency support

## Core Tables

**Transactions Table:**
```sql
CREATE TABLE Transactions (
    Id TEXT PRIMARY KEY,
    HledgerTransactionCode TEXT NOT NULL UNIQUE,
    Date TEXT NOT NULL,  -- ISO 8601 YYYY-MM-DD
    Payee TEXT NOT NULL,
    Amount INTEGER NOT NULL,  -- Cents: 4523 = $45.23
    CurrencyCode TEXT NOT NULL DEFAULT 'USD',
    Account TEXT NOT NULL,
    CategoryId TEXT NULL,  -- FK to Categories
    Category TEXT NOT NULL,  -- Denormalized for .hledger sync
    Memo TEXT NULL,
    Status TEXT NOT NULL CHECK(Status IN ('Pending', 'Cleared', 'Reconciled')),
    IsSplit INTEGER NOT NULL DEFAULT 0,
    ParentTransactionId TEXT NULL,
    Hash TEXT NOT NULL,  -- SHA256 for duplicate detection
    SyncStatus TEXT NOT NULL CHECK(SyncStatus IN ('InSync', 'PendingWrite', 'WriteError', 'ConflictDetected')),
    CreatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    UpdatedAt TEXT NOT NULL DEFAULT (datetime('now')),
    Source TEXT NOT NULL CHECK(Source IN ('Manual', 'CsvImport', 'ExternalEdit')),
    RowVersion INTEGER NOT NULL DEFAULT 1,  -- Optimistic concurrency

    FOREIGN KEY (ParentTransactionId) REFERENCES Transactions(Id) ON DELETE CASCADE,
    FOREIGN KEY (CategoryId) REFERENCES Categories(Id) ON DELETE SET NULL
);
```

**Other Core Tables:**
- **Accounts** - Account hierarchy (FullPath as primary key for hledger sync)
- **Categories** - User-friendly categorization with color/icon metadata
- **ImportRules** - Pattern matching for auto-categorization (Priority-ordered, Confidence stored as INTEGER 0-10000)
- **RecurringTransactions** - Detected patterns for predictions (UserConfirmed required)
- **CsvImports** - Import audit trail (FileHash prevents re-imports)
- **HledgerFileAudits** - File modification audit (BalanceChecksum as INTEGER)
- **CacheMetadata** - Single-row table tracking sync state (SchemaVersion field)
- **RecurringTransactionMatches** - Junction table

## Money Value Object Pattern

```csharp
public readonly struct Money
{
    private readonly long _cents;
    public string CurrencyCode { get; }

    public Money(decimal amount, string currencyCode = "USD")
    {
        _cents = (long)(amount * 100);  // $45.23 → 4523 cents
        CurrencyCode = currencyCode;
    }

    public decimal ToDecimal() => _cents / 100.0m;
    public long ToCents() => _cents;

    // Arithmetic operators...
}
```

## Optimistic Concurrency Example

```csharp
// EF Core checks: UPDATE Transactions SET Amount = 5000 WHERE Id = ? AND RowVersion = ?
try
{
    await _context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    throw new ConflictException(
        "Transaction was modified by external editor. Please refresh and try again.",
        ex
    );
}
```

## Schema Rationale

**Key Decisions:**
1. **INTEGER for money** - Zero rounding errors, exact balance checksums (vs. REAL floating point)
2. **RowVersion triggers** - Detects external .hledger edits, prevents data loss
3. **Dual tracking (CategoryId + Category)** - Referential integrity + .hledger sync
4. **TEXT for GUIDs** - SQLite standard practice
5. **JSON as TEXT** - ColumnMapping stored as JSON string (EF Core serializes)

**Full schema SQL available at:** `src/Ledgerly.Api/Common/Data/schema.sql`

---
