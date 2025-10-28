# Session Summary: Duplicate Detection Missing

**Date:** 2025-10-22
**Agent:** James (Dev Agent)
**Status:** Backend fixes complete, duplicate detection feature needs implementation

---

## Issues Fixed This Session

### 1. ✅ Ambient Transaction Error (FIXED)
**Problem:** SQLite doesn't support `TransactionScope` (ambient transactions)
```
An ambient transaction has been detected, but the current provider does not support ambient transactions
```

**Solution Applied:**
- File: `src/Ledgerly.Api/Features/ImportCsv/ConfirmImportHandler.cs`
- Changed from `TransactionScope` to EF Core's `_dbContext.Database.BeginTransactionAsync()`
- Added explicit `CommitAsync()` and `RollbackAsync()` calls
- Lines affected: 68-177

### 2. ✅ Missing Database Tables (FIXED)
**Problem:** Database didn't exist, causing "no such table: CsvImports" error

**Solution Applied:**
- File: `src/Ledgerly.Api/Program.cs`
- Added automatic migration on startup (lines 110-124)
- Database now auto-created at `ledgerly-cache.db` with all tables

---

## Current Issue: Duplicate Dialog Not Showing

### Root Cause Identified
The duplicate warning dialog doesn't appear because **the backend is not detecting duplicates**.

### What's Working:
1. ✅ Frontend has `DuplicateWarningDialogComponent` implemented
2. ✅ Frontend calls `checkForDuplicates()` after column mapping
   - Location: `src/Ledgerly.Web/src/app/features/import/import-csv.component.ts:403`
3. ✅ Frontend correctly shows "0 transactions ready to import" because API returns empty duplicates array

### What's Missing:
**Backend duplicate detection not implemented!**

File: `src/Ledgerly.Api/Features/ImportCsv/PreviewCsvHandler.cs`

Current response (lines 81-93) only sets:
- ✅ Headers
- ✅ SampleRows
- ✅ Column detection
- ✅ Manual mapping info
- ❌ **Duplicates** (always empty list)
- ❌ **Suggestions** (always empty list)

### Screenshots Analysis
User provided 4 screenshots showing CSV import flow:
1. **page1.png**: Upload CSV (working)
2. **page2.png**: Column mapping with "0 transactions ready to import" (shows empty duplicates)
3. **page3.png**: Review Import - all transactions marked "New" (no duplicates detected)
4. **page4.png**: Success message (import works)

**Missing:** Duplicate warning dialog that should appear between page 2 and page 3

---

## Implementation Plan for Next Session

### Task 1: Create Duplicate Detection Service
**File to create:** `src/Ledgerly.Api/Features/ImportCsv/DuplicateDetectionService.cs`

**Logic needed:**
```csharp
public interface IDuplicateDetectionService
{
    Task<List<DuplicateTransactionDto>> DetectDuplicates(
        List<Dictionary<string, string>> csvRows,
        Dictionary<string, string> columnMapping,
        CancellationToken ct);
}
```

**Fuzzy matching algorithm (from PRD):**
1. **Exact match (100%):** Date exact, Amount exact, Payee exact → Green checkmark
2. **High confidence (95%):** Date ±1 day, Amount exact, Payee normalized match → Yellow warning
3. **Medium confidence (80%):** Date ±3 days, Amount ±$0.05, Payee fuzzy match → Show for review
4. **Low confidence (<80%):** Don't flag as duplicate

**Query existing transactions from:**
- `_dbContext.Transactions` table (Transaction entity)
- Hash comparison using `TransactionHash` column
- Date range query for performance (±7 days from CSV date)

### Task 2: Create Category Suggestion Service
**File to create:** `src/Ledgerly.Api/Features/ImportCsv/CategorySuggestionService.cs`

**Logic needed:**
```csharp
public interface ICategorySuggestionService
{
    Task<List<CategorySuggestionDto>> SuggestCategories(
        List<Dictionary<string, string>> csvRows,
        Dictionary<string, string> columnMapping,
        CancellationToken ct);
}
```

**Query ImportRules table:**
- Match payee patterns against `ImportRules.PayeePattern`
- Return confidence based on `ImportRules.Confidence` column
- Support `MatchType`: Contains, StartsWith, EndsWith, Exact

### Task 3: Update PreviewCsvHandler
**File to modify:** `src/Ledgerly.Api/Features/ImportCsv/PreviewCsvHandler.cs`

**Changes needed (around line 75):**
```csharp
// Add duplicate detection
var duplicates = new List<DuplicateTransactionDto>();
var suggestions = new List<CategorySuggestionDto>();

if (columnDetection != null && columnDetection.AllRequiredFieldsDetected)
{
    duplicates = await _duplicateDetection.DetectDuplicates(
        result.SampleRows,
        columnDetection.DetectedMappings,
        linkedCts.Token);

    suggestions = await _categorySuggestion.SuggestCategories(
        result.SampleRows,
        columnDetection.DetectedMappings,
        linkedCts.Token);
}

// Update response
return new PreviewCsvResponse
{
    // ... existing fields ...
    Duplicates = duplicates,  // ADD THIS
    Suggestions = suggestions  // ADD THIS
};
```

### Task 4: Register Services in Program.cs
**File to modify:** `src/Ledgerly.Api/Program.cs`

Add around line 94:
```csharp
builder.Services.AddScoped<IDuplicateDetectionService, DuplicateDetectionService>();
builder.Services.AddScoped<ICategorySuggestionService, CategorySuggestionService>();
```

### Task 5: Test Duplicate Detection
**Test data needed:**
1. Import CSV with 3 transactions
2. Import same CSV again → should detect 3 exact duplicates
3. Import CSV with slightly different dates (±1 day) → should detect high confidence duplicates
4. Verify duplicate dialog appears with side-by-side comparison

---

## File Locations Reference

### Frontend (Angular)
- Import component: `src/Ledgerly.Web/src/app/features/import/import-csv.component.ts`
- Duplicate dialog: `src/Ledgerly.Web/src/app/features/import/duplicate-warning-dialog.component.ts`
- Duplicate dialog template: `src/Ledgerly.Web/src/app/features/import/duplicate-warning-dialog.component.html`
- Duplicate dialog styles: `src/Ledgerly.Web/src/app/features/import/duplicate-warning-dialog.component.scss`

### Backend (C# API)
- Preview handler: `src/Ledgerly.Api/Features/ImportCsv/PreviewCsvHandler.cs`
- Confirm handler: `src/Ledgerly.Api/Features/ImportCsv/ConfirmImportHandler.cs` (FIXED this session)
- Program.cs: `src/Ledgerly.Api/Program.cs` (FIXED this session)
- Response DTO: `src/Ledgerly.Contracts/Dtos/PreviewCsvResponse.cs`
- Duplicate DTO: `src/Ledgerly.Contracts/Dtos/DuplicateTransactionDto.cs`
- Category DTO: `src/Ledgerly.Contracts/Dtos/CategorySuggestionDto.cs`

### Database
- Transactions table: `Transactions` (for duplicate checking)
- Import rules: `ImportRules` (for category suggestions)
- Column mappings: `ColumnMappingRules` (already working)
- Database location: `ledgerly-cache.db` (in API running directory)

---

## Commands for Next Session

### Build and Run
```bash
# Build API
cd /home/ihsan/Desktop/Ledgerly
dotnet build src/Ledgerly.Api/Ledgerly.Api.csproj

# Run API (migrations auto-apply on startup)
dotnet run --project src/Ledgerly.Api/Ledgerly.Api.csproj

# Run Frontend
cd src/Ledgerly.Web
npm start
```

### Test Import
```bash
# Sample CSV location (if exists)
ls tests/test-data/*.csv

# Or use the quick-test.csv from screenshots (176 bytes)
# Location: Not in repo yet - user has it locally
```

---

## Related Stories/Documentation

### PRD References
- **Story 2.5:** Duplicate Detection & Category Suggestion (NOT YET IMPLEMENTED)
- **Story 2.6:** Import Preview and Confirmation (PARTIALLY IMPLEMENTED)

### Design Spec References
- Duplicate detection UI: See `.ai/screenshots/` for expected behavior
- Fuzzy matching algorithm: See `docs/front-end-spec.md` lines 429-450
- Confidence indicators: Green (exact), Yellow (likely), Gray (uncertain)

---

## Questions for Next Session

1. **Do you have existing transactions in the database to test duplicate detection?**
   - If no: Need to import same CSV twice to see duplicates

2. **Do you want category suggestions implemented at the same time?**
   - Both are in same response DTO
   - Can do duplicates first, then categories

3. **What test CSV data should we use?**
   - The quick-test.csv from screenshots?
   - Need specific format for testing

---

## Git Status
Current branch: `James-AI/3.1`

Modified files this session:
- `src/Ledgerly.Api/Features/ImportCsv/ConfirmImportHandler.cs` (transaction fix)
- `src/Ledgerly.Api/Program.cs` (auto-migration)

Untracked files:
- `.ai/screenshots/` (user-provided)
- This summary: `.ai/session-summary-duplicate-detection.md`

**Recommendation:** Commit the transaction fixes before implementing duplicate detection:
```bash
git add src/Ledgerly.Api/Features/ImportCsv/ConfirmImportHandler.cs
git add src/Ledgerly.Api/Program.cs
git commit -m "fix: Replace TransactionScope with EF Core transactions for SQLite compatibility

- Remove System.Transactions dependency causing ambient transaction errors
- Add automatic database migration on startup
- Fixes import confirmation failures on SQLite"
```

---

## Next Agent Recommendation

For implementing duplicate detection, use the **`*develop-story` command** with James (Dev agent) to:
1. Create duplicate detection service with tests
2. Create category suggestion service with tests
3. Update PreviewCsvHandler to call both services
4. Validate with end-to-end import test

The frontend is already complete and waiting for backend implementation.

---

**End of Session Summary**
