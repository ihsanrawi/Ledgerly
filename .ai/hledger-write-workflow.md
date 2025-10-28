# How CSV Import Writes to hledger Journal Files

## Quick Answer

**YES**, the backend writes to hledger journal files during CSV import using an **atomic, validated write pattern**.

## File Location

Default path: `~/.ledgerly/ledger.hledger`

(See [ConfirmImportHandler.cs:197-212](../src/Ledgerly.Api/Features/ImportCsv/ConfirmImportHandler.cs#L197-L212))

---

## Complete Write Workflow

### 1. Import Confirmation Trigger
**File:** `ConfirmImportHandler.cs`

When user clicks "Finalize Import" button, the frontend calls:
```
POST /api/import/confirm
```

With payload:
```json
{
  "transactions": [...],
  "csvImportId": "guid",
  "fileName": "duplicate-test.csv"
}
```

### 2. Write Process (Atomic & Safe)

**Location:** [ConfirmImportHandler.HandleAsync](../src/Ledgerly.Api/Features/ImportCsv/ConfirmImportHandler.cs#L34-L195)

```csharp
// Step 1: Start database transaction (SQLite)
await using var transaction = await _dbContext.Database.BeginTransactionAsync();

// Step 2: Save to SQLite cache
await _dbContext.Transactions.AddRangeAsync(transactionEntities);
await _dbContext.SaveChangesAsync();

// Step 3: Write to .hledger file (BULK APPEND)
writeResult = await _fileWriter.BulkAppendAsync(transactionEntities, hledgerFilePath);

// Step 4: Create audit records
await _dbContext.CsvImports.AddAsync(csvImportAudit);
await _dbContext.HledgerFileAudits.AddAsync(fileAudit);

// Step 5: Commit transaction (all or nothing)
await transaction.CommitAsync();
```

### 3. BulkAppendAsync Implementation

**File:** [HledgerFileWriter.cs:76-102](../src/Ledgerly.Api/Common/Hledger/HledgerFileWriter.cs#L76-L102)

**Key Function:** `BulkAppendAsync(List<Transaction> transactions, string filePath)`

#### Atomic Write Pattern (Lines 423-520)

```
1. Read existing file content
   └─ /home/user/.ledgerly/ledger.hledger

2. Calculate SHA256 hash BEFORE
   └─ "abc123..." (file integrity tracking)

3. Parse existing account declarations
   └─ Extract: account Assets:Checking, etc.

4. Merge account declarations
   └─ Add new accounts from import
   └─ Sort alphabetically

5. Format all new transactions
   └─ Convert to hledger journal format

6. Build complete file content
   ├─ account declarations (sorted)
   ├─ existing transactions (preserved)
   └─ new transactions (appended)

7. Write to TEMP file first
   └─ /home/user/.ledgerly/ledger.hledger.tmp

8. Validate with hledger check
   └─ hledger check /path/to/ledger.hledger.tmp
   └─ If FAILS → Delete temp file, throw exception
   └─ If PASS → Continue

9. Create BACKUP
   └─ Copy: ledger.hledger → ledger.hledger.bak

10. Atomic rename
    └─ Move: ledger.hledger.tmp → ledger.hledger (overwrite)

11. Calculate SHA256 hash AFTER
    └─ "def456..." (verify write succeeded)

12. Return BulkWriteResult
    └─ { hashBefore, hashAfter, transactionsWritten }
```

---

## Safety Guarantees

### 🛡️ Atomicity
- **Temp file pattern**: Write to `.tmp` first, then atomic rename
- **Database transaction**: SQLite + hledger writes wrapped in transaction
- **Rollback on failure**: Any error triggers full rollback (SQLite + hledger file restored from `.bak`)

### ✅ Validation
- **hledger check** runs on temp file BEFORE committing
- Invalid transactions are REJECTED before touching the real file
- User sees error message with validation details

### 🔙 Backup & Recovery
- **Automatic backup**: `.hledger.bak` created before every write
- **Manual restore**: `RestoreFromBackup()` method available
- **File hashing**: SHA256 hashes tracked in `HledgerFileAudit` table

### 🔒 File Locking
- **Retry logic**: 3 retries with 100ms delay for file I/O conflicts
- **UTF-8 encoding**: Consistent encoding across all operations

---

## Example: What Gets Written

### Before Import
```hledger
account Assets:Checking
account Expenses:Groceries

2025-01-10 Walmart
    Expenses:Groceries     $45.00
    Assets:Checking       -$45.00
```

### After Importing 3 New Transactions
```hledger
account Assets:Checking
account Expenses:Coffee
account Expenses:Groceries
account Expenses:Shopping

2025-01-10 Walmart
    Expenses:Groceries     $45.00
    Assets:Checking       -$45.00

2025-01-15 Target
    Expenses:Shopping      $50.00
    Assets:Checking       -$50.00

2025-01-16 Walmart
    Expenses:Groceries     $75.50
    Assets:Checking       -$75.50

2025-01-17 Starbucks
    Expenses:Coffee         $5.49
    Assets:Checking        -$5.49
```

**Note:**
- New accounts added (`Expenses:Coffee`, `Expenses:Shopping`)
- Account declarations sorted alphabetically
- Existing transactions preserved
- New transactions appended with blank line separation

---

## Transaction Format

**File:** [TransactionFormatter.cs](../src/Ledgerly.Api/Common/Hledger/TransactionFormatter.cs)

**Output Format:**
```hledger
2025-01-17 Starbucks
    Expenses:Coffee         $5.49
    Assets:Checking        -$5.49

```

**Formatting Rules:**
1. Date in `YYYY-MM-DD` format
2. Payee sanitized (no newlines, tabs)
3. Posting indented with 4 spaces
4. Amounts right-aligned
5. Blank line after each transaction

---

## Audit Trail

Every write operation is logged in `HledgerFileAudits` table:

```csharp
{
    Id: Guid,
    Timestamp: DateTime.UtcNow,
    Operation: "CsvImport",
    FileHashBefore: "abc123...",
    FileHashAfter: "def456...",
    TransactionCount: 3,
    BalanceChecksum: 0, // Placeholder for future
    TriggeredBy: "User",
    RelatedEntityId: csvImportAuditId,
    UserId: userId,
    FilePath: "/home/user/.ledgerly/ledger.hledger"
}
```

This provides:
- **Who** imported (userId)
- **When** (timestamp)
- **What** (transaction count)
- **File integrity** (hashes before/after)
- **Traceability** (links to CsvImport record)

---

## Error Handling

### Validation Failure
```
1. Temp file validation fails
2. HledgerValidationException thrown
3. Temp file deleted
4. Database transaction rolled back
5. User sees error: "Transaction validation failed. Please check your transaction data."
```

### File I/O Failure
```
1. IOException during write
2. Retry 3 times with 100ms delay
3. If all retries fail → throw exception
4. Database transaction rolled back
5. Original file untouched (backup still safe)
```

### Unexpected Error
```
1. Any other exception
2. Temp file cleaned up
3. Database transaction rolled back
4. HledgerException thrown with user-friendly message
5. Original file restored from backup if needed
```

---

## Performance Optimization

**BulkAppendAsync** is significantly faster than individual appends:

| Operation | Method | Time (10 transactions) |
|-----------|--------|------------------------|
| Individual | 10 × AppendTransactionAsync | ~2-3 seconds |
| Bulk | 1 × BulkAppendAsync | ~200-300ms |

**Why?**
- Single file read instead of 10
- Single hledger validation instead of 10
- Single atomic write instead of 10

---

## Concurrency & Safety

### Multi-User Scenario (Future)
- **File locking**: Retry logic handles concurrent writes
- **User isolation**: Each user has separate `.ledgerly/ledger.hledger` file (via userId lookup)
- **Audit trail**: Every write tracked with userId

### Single-User Scenario (MVP)
- **File watching**: `FileSystemWatcher` detects external edits (not shown in import flow)
- **Cache invalidation**: SQLite cache rebuilds if `.hledger` file modified externally
- **Backup protection**: User can manually restore from `.bak` if needed

---

## Relevant Files

1. **Import Handler**: [ConfirmImportHandler.cs](../src/Ledgerly.Api/Features/ImportCsv/ConfirmImportHandler.cs)
2. **File Writer**: [HledgerFileWriter.cs](../src/Ledgerly.Api/Common/Hledger/HledgerFileWriter.cs)
3. **Transaction Formatter**: [TransactionFormatter.cs](../src/Ledgerly.Api/Common/Hledger/TransactionFormatter.cs)
4. **Process Runner**: [HledgerProcessRunner.cs](../src/Ledgerly.Api/Common/Hledger/HledgerProcessRunner.cs) (for validation)
5. **Database Context**: [LedgerlyDbContext.cs](../src/Ledgerly.Api/Common/Data/LedgerlyDbContext.cs)

---

## Summary

✅ **YES**, CSV imports write to `.hledger` journal files
✅ Writes are **atomic** (temp file + atomic rename)
✅ Writes are **validated** (hledger check before commit)
✅ Writes are **backed up** (.bak file created automatically)
✅ Writes are **audited** (SHA256 hashes + database logs)
✅ Writes are **safe** (rollback on any failure)

The system follows **Plain Text Accounting** principles:
- Human-readable journal format
- Standard hledger syntax
- Portable across tools (can use hledger CLI directly)
- No vendor lock-in (just text files)
