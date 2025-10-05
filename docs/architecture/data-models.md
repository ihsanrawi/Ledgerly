# Data Models

## Transaction

**Purpose:** Represents a single financial transaction in the double-entry accounting system. Core entity for all financial data with hledger synchronization support.

**Key Attributes:**
- `Id`: Guid - Cache identifier
- `HledgerTransactionCode`: Guid - Embedded in .hledger file as transaction code for identity mapping
- `Date`: DateTime - Transaction date (ISO 8601: YYYY-MM-DD)
- `Payee`: string - Merchant/person (e.g., "Whole Foods", "Amazon")
- `Amount`: decimal - Transaction amount (positive for income/expenses)
- `Account`: string - hledger account full path (e.g., "Assets:Checking")
- `Category`: string - User-friendly categorization
- `Memo`: string? - Optional description/notes
- `Status`: enum (Pending, Cleared, Reconciled) - Reconciliation status
- `IsSplit`: bool - Indicates if transaction has multiple postings
- `ParentTransactionId`: Guid? - Links split postings to parent transaction
- `Hash`: string - Duplicate detection (SHA256 of date+payee+amount for FR4)
- `SyncStatus`: enum (InSync, PendingWrite, WriteError, ConflictDetected) - Cache synchronization state
- `CreatedAt`: DateTime - Audit timestamp
- `UpdatedAt`: DateTime - Last modification timestamp
- `Source`: enum (Manual, CsvImport, ExternalEdit) - Origin tracking

**Relationships:**
- Belongs to one **Category** (classification)
- Belongs to one **Account** (posting source)
- May have associated **ImportRule** (if categorized via suggestion)
- May match a **RecurringTransaction** pattern
- Self-referential: Parent transaction has many child splits (`ParentTransactionId`)

**Design Decisions:**
- **`HledgerTransactionCode`:** Embeds Guid in .hledger file as transaction code (e.g., `2025-01-15 (abc-123) Whole Foods`). Solves identity problem for edits. hledger-native feature.
- **`SyncStatus`:** Tracks cache consistency. If .hledger write fails, status = `WriteError` → retry logic triggered.
- **Split support:** Read-only in MVP. When parsing .hledger, multi-posting transactions create parent + children with `ParentTransactionId`. UI displays but doesn't create splits.

**hledger File Example:**
```
2025-01-15 (550e8400-e29b-41d4-a716-446655440000) Whole Foods
    Expenses:Groceries    $45.23
    Assets:Checking
```

## Account

**Purpose:** Represents hledger account hierarchy. Simplified to use path strings as canonical representation.

**Key Attributes:**
- `Id`: Guid - Cache identifier
- `FullPath`: string - Complete hledger account path (e.g., "Expenses:Groceries:Organic") - **PRIMARY KEY for hledger sync**
- `Type`: enum (Asset, Liability, Equity, Income, Expense) - Account classification
- `Balance`: decimal - Current balance (cached from hledger calculations)
- `IsActive`: bool - User-defined visibility flag
- `CreatedAt`: DateTime - Audit timestamp
- `LastSyncedAt`: DateTime - Last hledger balance refresh timestamp

**Relationships:**
- Has many **Transactions** (postings)
- **Hierarchy computed on-the-fly** from `FullPath` string (no foreign key hierarchy)

**Design Decisions:**
- **Removed `ParentAccountId`:** Eliminates synchronization complexity. Parent derived from path: `"Expenses:Groceries:Organic".Split(':').SkipLast(1) → "Expenses:Groceries"`
- **`FullPath` as source of truth:** Matches hledger account declarations. No translation layer needed.
- **Balance caching:** Queried via `hledger bal -O json`, cached for dashboard performance. Invalidated on .hledger file changes.

**Computed Properties (Application Layer):**
- `ParentPath`: string - Derived from `FullPath` (e.g., "Expenses:Groceries" from "Expenses:Groceries:Organic")
- `DisplayName`: string - Last segment only (e.g., "Organic" from full path)
- `Depth`: int - Hierarchy level (e.g., 3 for "Expenses:Groceries:Organic")

## Category

**Purpose:** User-friendly classification for transactions, distinct from hledger accounts. Simplifies UI interactions.

**Key Attributes:**
- `Id`: Guid - Cache identifier
- `Name`: string - Display name (e.g., "Groceries", "Dining Out", "Salary")
- `Type`: enum (Expense, Income, Transfer) - Classification
- `Color`: string - Hex color for UI charts (e.g., "#1ABC9C")
- `Icon`: string? - Optional Material Icon name (e.g., "shopping_cart")
- `MappedAccount`: string - Corresponding hledger account full path (e.g., "Expenses:Groceries")
- `IsDefault`: bool - Seeded categories vs. user-created
- `DisplayOrder`: int - Sort order in UI dropdowns
- `IsActive`: bool - Soft delete flag

**Relationships:**
- Has many **Transactions**
- Has many **ImportRules** (category suggestion triggers)
- Has many **RecurringTransactions**

**Design Decisions:**
- **Categories vs. Accounts:** Categories are UI abstraction; Accounts are hledger-native. Category "Groceries" maps to "Expenses:Groceries" account.
- **Default categories seeded:** 15-20 common categories created on first launch (Groceries, Rent, Utilities, Salary, etc.)

## ImportRule

**Purpose:** Pattern-matching rules for automatic transaction categorization (FR6). Learns from user corrections.

**Key Attributes:**
- `Id`: Guid - Cache identifier
- `PayeePattern`: string - Pattern to match (e.g., "WHOLE FOODS", "AMAZON%")
- `MatchType`: enum (Exact, Contains, StartsWith, EndsWith, Regex) - Matching strategy
- `Priority`: int - Rule application order (1 = highest priority, stop on first match)
- `SuggestedCategoryId`: Guid - Category to suggest when pattern matches
- `Confidence`: decimal (0.0-1.0) - Accuracy score (incremented when user accepts suggestion)
- `TimesApplied`: int - Usage count (metric for learning effectiveness)
- `TimesAccepted`: int - User confirmation count
- `IsActive`: bool - User can disable rules
- `CreatedAt`: DateTime - Rule creation timestamp
- `LastUsedAt`: DateTime? - Last application timestamp

**Relationships:**
- Belongs to one **Category** (suggestion target)
- Applied to many **Transactions** (via categorization workflow)

**Design Decisions:**
- **`Priority` field:** Addresses performance risk. High-priority rules (user-created, high confidence) evaluated first. Prevents 1M+ pattern match problem.
- **Simple matching in MVP:** Regex support exists but not exposed in UI. Phase 2 adds ML-based learning (FR7).
- **Confidence calculation:** `Confidence = TimesAccepted / TimesApplied`. Rules with <0.3 confidence archived after 30 days.

## RecurringTransaction

**Purpose:** Detected recurring transaction patterns for cash flow predictions (FR17, FR18). User-confirmed to prevent false positives.

**Key Attributes:**
- `Id`: Guid - Cache identifier
- `Payee`: string - Recurring payee (e.g., "Netflix", "Rent")
- `NormalizedPayee`: string - Canonical payee name (e.g., "Amazon" for "Amazon.com", "AMAZON MKTP", "Amazon Prime")
- `AverageAmount`: decimal - Typical amount (±10% variance allowed)
- `Frequency`: enum (Weekly, Biweekly, Monthly, Quarterly, Yearly) - Recurrence pattern
- `ExpectedDayOfMonth`: int? - For monthly patterns (e.g., 1 for rent, 15 for salary)
- `NextExpectedDate`: DateTime - Predicted next occurrence
- `CategoryId`: Guid - Associated category
- `Confidence`: decimal (0.0-1.0) - Pattern reliability (based on consistency of dates/amounts)
- `UserConfirmed`: bool - User has verified this is legitimate recurring transaction (prevents false positive predictions)
- `OccurrenceCount`: int - Number of historical matches (minimum 4 required)
- `LastOccurrenceDate`: DateTime - Most recent transaction date
- `IsActive`: bool - User can disable patterns
- `CreatedAt`: DateTime - Pattern detection timestamp

**Relationships:**
- Belongs to one **Category**
- References multiple **Transactions** (historical matches via intermediate table)

**Design Decisions:**
- **`UserConfirmed` required for predictions:** Detected patterns don't appear in Cash Flow Timeline until user confirms. UI: "We detected 5 recurring transactions. Review and confirm."
- **`NormalizedPayee`:** Solves duplicate payee problem ("Amazon.com" vs "AMAZON MKTP"). Simple normalization in MVP (lowercase, remove special chars). Phase 2 adds ML-based merchant recognition.
- **Tightened detection:** Requires 4+ occurrences (not 3) and confidence >0.85 to reduce false positives.

## CsvImport

**Purpose:** Audit trail for CSV import operations. Tracks import history for troubleshooting and duplicate detection.

**Key Attributes:**
- `Id`: Guid - Cache identifier
- `FileName`: string - Original CSV filename
- `ImportedAt`: DateTime - Import timestamp
- `TotalRows`: int - CSV row count
- `SuccessfulImports`: int - Transactions successfully written to .hledger
- `DuplicatesSkipped`: int - Duplicate transaction count (FR4)
- `ErrorCount`: int - Parse/validation error count
- `BankFormat`: string? - Detected bank format (e.g., "Chase", "Bank of America")
- `ColumnMapping`: JSON - Stored mapping for future imports (FR3: manual mapping persistence)
- `FileHash`: string - SHA256 of CSV file (prevents re-importing same file)

**Relationships:**
- Has many **Transactions** (via `ImportId` foreign key on Transaction)

**Design Decisions:**
- **Column mapping storage:** JSON field stores user's manual mappings. Reused for same bank's future imports (matched by `BankFormat` or filename pattern).
- **Import deduplication:** `FileHash` prevents accidental re-import of same CSV file.

## HledgerFileAudit

**Purpose:** Audit trail for all .hledger file modifications. Critical for debugging and user trust in financial data integrity.

**Key Attributes:**
- `Id`: Guid - Cache identifier
- `Timestamp`: DateTime - Operation timestamp
- `Operation`: enum (CsvImport, TransactionAdd, TransactionEdit, TransactionDelete, ExternalEdit, CacheRebuild) - Operation type
- `FileHashBefore`: string - SHA256 of .hledger file before modification
- `FileHashAfter`: string - SHA256 after modification
- `TransactionCount`: int - Total transactions in file after operation
- `BalanceChecksum`: decimal - Sum of all account balances (quick consistency check)
- `TriggeredBy`: enum (User, System, External) - Operation source
- `ErrorMessage`: string? - If operation failed, error details
- `RelatedEntityId`: Guid? - Transaction/Import ID that triggered operation

**Relationships:**
- May reference **Transaction** (for transaction-specific operations)
- May reference **CsvImport** (for import operations)

**Design Decisions:**
- **Financial app requirement:** Audit trails build user trust. "Show History" feature in UI lets users see all file changes.
- **Retention policy:** Keep last 30 days detailed logs, summarize to monthly after.
- **Consistency validation:** `BalanceChecksum` enables quick detection of cache drift from .hledger file.

## CacheMetadata

**Purpose:** Tracks SQLite cache synchronization state with .hledger files. Solves dual-state problem.

**Key Attributes:**
- `Id`: int - Single row (PK = 1)
- `HledgerFilePath`: string - Path to .hledger source file
- `HledgerFileHash`: string - SHA256 of current .hledger file
- `LastSyncedAt`: DateTime - Last successful cache rebuild timestamp
- `TransactionCount`: int - Cached transaction count
- `SyncStatus`: enum (InSync, OutOfSync, Rebuilding, Error) - Overall cache state
- `HledgerVersion`: string - Version of hledger binary used (e.g., "1.32.3")
- `LastErrorMessage`: string? - Most recent sync error

**Design Decisions:**
- **Single-row table:** Only one .hledger file per app instance in MVP. Phase 2 may support multiple files.
- **Hash validation:** On app startup, compare `HledgerFileHash` with actual file SHA256. If mismatch → trigger cache rebuild.
- **Cache rebuild strategy:** If `OutOfSync`, parse .hledger file, re-execute hledger queries, rebuild all cache tables.

---
