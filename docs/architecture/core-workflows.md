# Core Workflows

This section documents the critical user journeys with sequence diagrams showing interactions between frontend, backend, hledger binary, and data storage.

## CSV Import Workflow

**User Story:** User uploads bank CSV file, maps columns, reviews suggestions, confirms import.

**Success Criteria:**
- >95% successful transaction parsing (NFR9)
- Duplicate detection 100% accurate (FR4)
- Adaptive learning creates rules from corrections (FR6, FR7)

```mermaid
sequenceDiagram
    participant U as User (Angular UI)
    participant API as ASP.NET API
    participant Bus as Wolverine Bus
    participant Import as ImportCsv Slice
    participant Rules as AdaptiveLearningService
    participant Writer as HledgerFileWriter
    participant Runner as HledgerProcessRunner
    participant Files as .hledger Files
    participant Cache as SQLite Cache
    participant Binary as hledger Binary

    U->>API: POST /api/import/csv (file upload)
    API->>Bus: ImportCsvCommand
    Bus->>Import: Handle ImportCsvCommand

    Note over Import: Parse CSV with CsvHelper
    Import->>Import: Detect bank format (Chase, BofA, etc.)
    Import->>Import: Auto-map columns (amount, date, payee)

    Import-->>U: Preview with 5 sample rows
    U->>API: POST /api/import/preview-confirm (column mappings)
    API->>Bus: ConfirmImportCommand
    Bus->>Import: Handle ConfirmImportCommand

    loop For each CSV row
        Import->>Import: Parse transaction
        Import->>Import: Calculate SHA256 hash (duplicate check)
        Import->>Cache: Check if hash exists
        Cache-->>Import: Duplicate status

        alt Is duplicate
            Import->>Import: Skip transaction, increment DuplicatesSkipped
        else Is new transaction
            Import->>Rules: GetCategorySuggestion(payee)
            Rules->>Cache: Query ImportRules (priority order)
            Cache-->>Rules: Matching rules
            Rules-->>Import: SuggestedCategory (with confidence)

            Import->>Import: Create Transaction model with suggestion
            Import->>Writer: AppendTransaction(transaction)
            Writer->>Files: Write to .hledger.tmp
            Writer->>Runner: ValidateFile(.hledger.tmp)
            Runner->>Binary: Execute "hledger check"
            Binary->>Files: Read .hledger.tmp
            Binary-->>Runner: Validation result

            alt Validation success
                Writer->>Files: Atomic rename .tmp → .hledger
                Writer->>Files: Create .hledger.bak
                Writer-->>Import: WriteSuccess
                Import->>Cache: Insert transaction with SyncStatus=InSync
            else Validation failure
                Writer-->>Import: WriteError with details
                Import->>Import: Log error, increment ErrorCount
            end
        end
    end

    Import->>Cache: Insert CsvImport audit record
    Import-->>U: Import summary (successful/duplicates/errors)

    U->>U: Review categorization suggestions

    alt User accepts suggestion
        U->>API: POST /api/categorize/confirm
        API->>Bus: ConfirmCategorizationCommand
        Bus->>Rules: LearnFromAcceptance(ruleId)
        Rules->>Cache: Increment TimesAccepted, boost Confidence
    else User corrects category
        U->>API: POST /api/categorize/correct
        API->>Bus: CorrectCategorizationCommand
        Bus->>Rules: LearnFromRejection(ruleId, correctCategoryId, payee)
        Rules->>Cache: Decrease Confidence, create competing rule if pattern detected
        Rules->>Writer: UpdateTransaction (new category)
        Writer->>Files: Rewrite .hledger file
    end
```

**Performance Targets:**
- 1000 transactions: <5 seconds import time
- Duplicate detection: O(n) with hash lookup
- Categorization suggestions: <50ms per transaction (priority-ordered rule matching)

---

## Dashboard Load with Cash Flow Timeline

**User Story:** User opens dashboard, sees net worth, expense breakdown, income/expense chart, and **Cash Flow Timeline with predictions**.

**Success Criteria:**
- Dashboard loads in <2 seconds (NFR1)
- Cash Flow Timeline shows 30/60/90 day predictions
- Overdraft alerts displayed if predicted balance <0

```mermaid
sequenceDiagram
    participant U as User (Angular UI)
    participant API as ASP.NET API
    participant Bus as Wolverine Bus
    participant Dashboard as GetDashboard Slice
    participant Runner as HledgerProcessRunner
    participant Cache as SQLite Cache
    participant Binary as hledger Binary
    participant Files as .hledger Files

    U->>API: GET /api/dashboard
    API->>Bus: GetDashboardQuery
    Bus->>Dashboard: Handle GetDashboardQuery

    par Parallel Widget Queries
        Dashboard->>Cache: GetNetWorth (account balances from cache)
        Cache-->>Dashboard: Net worth value
    and
        Dashboard->>Cache: GetExpenseBreakdown (last 30 days, grouped by category)
        Cache-->>Dashboard: Expense data
    and
        Dashboard->>Cache: GetIncomeExpense (last 12 months)
        Cache-->>Dashboard: Income/expense trend
    and
        Dashboard->>Cache: GetRecurringTransactions (UserConfirmed=true)
        Cache-->>Dashboard: Recurring patterns

        Dashboard->>Dashboard: Calculate Cash Flow Timeline
        Note over Dashboard: Algorithm:<br/>1. Start with current balance<br/>2. For each day (next 30/60/90):<br/>   - Check for recurring tx on this date<br/>   - Add expected income/subtract expected expense<br/>   - Calculate predicted balance<br/>   - Flag if balance < 0 (overdraft alert)

        Dashboard->>Dashboard: Generate alerts
        Note over Dashboard: Alert types:<br/>- Overdraft (balance < 0)<br/>- Large expense upcoming<br/>- Income gap detected
    end

    Dashboard->>Dashboard: Aggregate dashboard data
    Dashboard-->>U: Dashboard JSON (widgets + cashflow timeline + alerts)

    U->>U: Render dashboard with Chart.js
    U->>U: Display Cash Flow Timeline with visual markers

    Note over U: Visual markers:<br/>- Green dots = income<br/>- Red dots = expenses<br/>- Orange = overdraft warning<br/>- Category icons on timeline
```

**Caching Strategy:**
- Account balances cached on app start (refreshed on .hledger file change)
- Recurring transactions cached (refreshed nightly via scheduled job)
- Dashboard queries hit cache only (no hledger CLI execution for <2s load)

---

## Add Transaction Manually

**User Story:** User adds transaction via UI form (date, payee, amount, category).

**Success Criteria:**
- Transaction written to .hledger file with atomic operations
- 100% validation via hledger binary (NFR14)
- Cache updated immediately for instant UI refresh

```mermaid
sequenceDiagram
    participant U as User (Angular UI)
    participant API as ASP.NET API
    participant Bus as Wolverine Bus
    participant ManageTx as ManageTransactions Slice
    participant Writer as HledgerFileWriter
    participant Runner as HledgerProcessRunner
    participant Files as .hledger Files
    participant Cache as SQLite Cache
    participant Binary as hledger Binary
    participant Audit as HledgerFileAudit

    U->>API: POST /api/transactions (AddTransactionCommand)
    API->>Bus: AddTransactionCommand
    Bus->>ManageTx: Handle AddTransactionCommand

    ManageTx->>ManageTx: Validate input (FluentValidation)
    Note over ManageTx: Validation rules:<br/>- Date not in future<br/>- Amount > 0<br/>- Payee required<br/>- Account exists

    ManageTx->>ManageTx: Generate HledgerTransactionCode (Guid)
    ManageTx->>ManageTx: Map Category → hledger Account

    ManageTx->>Writer: AppendTransaction(transaction, filePath)
    Writer->>Files: Read current .hledger file
    Writer->>Files: Calculate SHA256 (before hash)
    Writer->>Files: Write to .hledger.tmp
    Note over Writer: Format:<br/>2025-01-15 (guid) Payee<br/>    Expenses:Groceries  $45.23<br/>    Assets:Checking

    Writer->>Runner: ValidateFile(.hledger.tmp)
    Runner->>Binary: Execute "hledger check .hledger.tmp"
    Binary->>Files: Read .hledger.tmp
    Binary-->>Runner: Validation result

    alt Validation success
        Runner-->>Writer: Valid
        Writer->>Files: Create .hledger.bak (backup)
        Writer->>Files: Atomic rename .tmp → .hledger
        Writer->>Files: Calculate SHA256 (after hash)

        Writer->>Audit: Log file modification
        Audit->>Cache: Insert HledgerFileAudit record
        Note over Audit: Operation=TransactionAdd<br/>FileHashBefore, FileHashAfter<br/>TriggeredBy=User

        Writer-->>ManageTx: WriteSuccess

        ManageTx->>Cache: Insert transaction
        ManageTx->>Cache: Update CacheMetadata (new hash, LastSyncedAt)

        ManageTx-->>U: Success (transaction ID)
        U->>U: Refresh transaction list (optimistic UI update)

    else Validation failure
        Runner-->>Writer: ValidationError (details)
        Writer->>Files: Delete .hledger.tmp
        Writer-->>ManageTx: WriteError
        ManageTx-->>U: Error message with hledger error details
        Note over U: Example error:<br/>"Transaction unbalanced:<br/>Expenses:Groceries  $45.23<br/>Assets:Checking     $0.00"
    end
```

**Error Handling:**
- FluentValidation errors returned immediately (no file I/O)
- hledger validation errors preserved exactly as CLI output
- Rollback automatic (atomic rename fails → .tmp file deleted)
- Backup always created before modification

---

## External .hledger File Edit Detection

**User Story:** User edits .hledger file in VS Code while app is running. App detects change, rebuilds cache, shows notification.

**Success Criteria:**
- FileSystemWatcher detects changes within 500ms
- Cache rebuild completes within 2 seconds for <10K transactions
- UI shows non-blocking notification ("Ledger file updated externally. Refreshing...")

```mermaid
sequenceDiagram
    participant External as External Editor (VS Code)
    participant Files as .hledger Files
    participant Watcher as FileSystemWatcher
    participant Sync as CacheSynchronizer
    participant Parser as HledgerFileParser
    participant Runner as HledgerProcessRunner
    participant Cache as SQLite Cache
    participant Binary as hledger Binary
    participant UI as Angular UI (WebSocket)

    External->>Files: Save modified .hledger file
    Files->>Watcher: FileChanged event
    Note over Watcher: Debounce 500ms<br/>(wait for editor to finish writing)

    Watcher->>Sync: OnFileChanged(filePath)
    Sync->>Files: Calculate SHA256 hash
    Sync->>Cache: Get CacheMetadata
    Cache-->>Sync: Current HledgerFileHash

    alt Hash mismatch (file changed)
        Sync->>Cache: Set SyncStatus=OutOfSync
        Sync->>UI: Send notification via SignalR (optional future)
        Note over UI: Show toast:<br/>"Ledger file updated externally.<br/>Refreshing..."

        Sync->>Cache: Set SyncStatus=Rebuilding
        Sync->>Parser: ParseFile(filePath)
        Parser->>Files: Read .hledger file line by line

        loop For each transaction in file
            Parser->>Parser: Parse transaction block
            Parser->>Parser: Extract HledgerTransactionCode (guid from transaction code)
            Parser->>Parser: Create Transaction model
        end

        Parser-->>Sync: List<Transaction>

        Sync->>Runner: GetBalances(filePath)
        Runner->>Binary: Execute "hledger bal -O json"
        Binary->>Files: Read .hledger file
        Binary-->>Runner: JSON balance data
        Runner-->>Sync: Account balances

        Sync->>Cache: Begin transaction
        Sync->>Cache: DELETE FROM Transactions
        Sync->>Cache: Bulk INSERT parsed transactions
        Sync->>Cache: UPDATE Accounts (balances)
        Sync->>Cache: UPDATE CacheMetadata (new hash, LastSyncedAt, SyncStatus=InSync)
        Sync->>Cache: Commit transaction

        Sync->>UI: Send cache-refreshed event
        UI->>UI: Reload dashboard/transaction list
        UI->>UI: Show success toast ("Refreshed successfully")

    else Hash match (no change)
        Sync->>Sync: Ignore event (spurious file watcher trigger)
    end
```

**Edge Cases Handled:**
- Multiple rapid saves → debounced to single rebuild
- File deleted → show error, preserve cache, disable writes
- File corrupted → show hledger error, preserve cache
- Concurrent writes (app + external) → last write wins, cache rebuilds

---

## Recurring Transaction Detection (Scheduled Job)

**User Story:** System runs nightly job to detect recurring patterns. User reviews/confirms patterns in UI.

**Success Criteria:**
- Nightly execution at 2 AM (Wolverine scheduled job)
- Minimum 4 occurrences + >0.85 confidence
- User confirmation required before predictions appear in Cash Flow Timeline

```mermaid
sequenceDiagram
    participant Scheduler as Wolverine Scheduler
    participant Bus as Wolverine Bus
    participant Detect as DetectRecurring Slice
    participant Cache as SQLite Cache
    participant Recurring as RecurringTransaction Repository

    Note over Scheduler: Daily at 2:00 AM
    Scheduler->>Bus: DetectRecurringCommand (scheduled)
    Bus->>Detect: Handle DetectRecurringCommand

    Detect->>Cache: Get transactions (last 6 months)
    Cache-->>Detect: List<Transaction>

    Detect->>Detect: Group by NormalizedPayee
    Note over Detect: Normalization:<br/>- Lowercase<br/>- Remove special chars<br/>- Common patterns:<br/>  "AMAZON.COM" → "amazon"<br/>  "Netflix.com" → "netflix"

    loop For each payee group (≥4 transactions)
        Detect->>Detect: Analyze date intervals
        Note over Detect: Frequency detection:<br/>- Weekly: 6-8 days apart<br/>- Biweekly: 13-15 days<br/>- Monthly: 28-32 days<br/>- Quarterly: 88-92 days

        Detect->>Detect: Calculate average amount
        Detect->>Detect: Calculate confidence
        Note over Detect: Confidence formula:<br/>- Date consistency: StdDev(intervals) < 2 days → +0.4<br/>- Amount consistency: StdDev(amounts) < 10% → +0.3<br/>- Occurrence count: ≥6 → +0.2, ≥4 → +0.1<br/>- Category consistency: same → +0.1

        alt Confidence > 0.85
            Detect->>Detect: Predict next occurrence date
            Detect->>Cache: Check if RecurringTransaction exists

            alt Pattern already exists
                Detect->>Recurring: Update (NextExpectedDate, Confidence, OccurrenceCount)
            else New pattern
                Detect->>Recurring: Insert new RecurringTransaction (UserConfirmed=false)
            end
        end
    end

    Detect->>Detect: Count new patterns detected
    Detect->>Cache: Log detection run (timestamp, patterns found)
    Detect-->>Scheduler: Job complete (summary)

    Note over Detect: User reviews in UI later
```

**User Confirmation Flow:**
```mermaid
sequenceDiagram
    participant U as User (Angular UI)
    participant API as ASP.NET API
    participant Recurring as RecurringTransaction Repository
    participant Cache as SQLite Cache

    U->>API: GET /api/predictions/recurring?confirmed=false
    API->>Recurring: Get unconfirmed patterns
    Recurring-->>U: List of detected patterns

    U->>U: Review pattern (payee, frequency, amount)

    alt User confirms pattern
        U->>API: POST /api/predictions/confirm/{id}
        API->>Recurring: Update UserConfirmed=true
        Recurring->>Cache: UPDATE RecurringTransaction
        API-->>U: Success
        Note over U: Pattern now appears in<br/>Cash Flow Timeline predictions
    else User rejects pattern
        U->>API: DELETE /api/predictions/reject/{id}
        API->>Recurring: Soft delete (IsActive=false)
        API-->>U: Success
    end
```

---
