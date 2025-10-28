# Fix: Review Duplicates Button Not Showing

## Problem Analysis

**Issue:** System detects duplicates correctly (backend logs show 3 duplicates found), but the "Review Duplicates" button does not appear in the UI.

**Root Cause:** Missing button condition in the template. The component had logic for:
- ✅ No duplicates → "Review Import" button
- ✅ Duplicates reviewed → "Finalize Import" button
- ❌ **Duplicates detected but NOT reviewed → NO BUTTON** (the bug!)

## Evidence from Logs

```
[07:28:54 INF] Duplicate detection complete. Found 3 duplicates out of 10 transactions
[07:28:54 INF] Duplicate detection found 3 duplicates out of 10 transactions
```

Backend correctly detected 3 duplicates but frontend had no way for user to review them.

## Solution Applied

**File Modified:** `src/Ledgerly.Web/src/app/features/import/import-csv.component.html`

**Changes:** Added a new button that shows when duplicates are detected but haven't been reviewed yet:

```html
<!-- Duplicates detected but not reviewed yet - show review button -->
<button
  mat-raised-button
  color="warn"
  (click)="checkForDuplicates()"
  [disabled]="!canProceedToNextStep()"
  *ngIf="canProceedToNextStep() && !requiresManualMapping() && hasDuplicates() && duplicateDecisions().size === 0">
  <mat-icon>warning</mat-icon>
  Review Duplicates
</button>
```

**Button Visibility Logic:**
1. **"Review Import"** - Shows when NO duplicates detected
2. **"Review Duplicates"** (NEW) - Shows when duplicates detected BUT not reviewed yet (`duplicateDecisions().size === 0`)
3. **"Finalize Import"** - Shows when duplicates reviewed (`duplicateDecisions().size > 0`)

## Button Flow Diagram

```
Upload CSV
   ↓
Preview Data
   ↓
Has Duplicates?
   ├─ NO  → [Review Import] button → Skip to finalize
   └─ YES → [Review Duplicates] button (🔴 color=warn with warning icon)
                ↓
          User reviews duplicates
                ↓
          [Finalize Import] button
```

## Testing Steps

1. Upload the `duplicate-test.csv` file (contains 10 rows with 3 duplicates)
2. After preview loads, verify you see:
   - Import Summary: "0 transactions ready to import"
   - Warning message: "Please review duplicate transactions before finalizing import"
   - **⚠️ [Review Duplicates] button** (yellow/warn color with warning icon)
3. Click "Review Duplicates" button
4. Duplicate warning dialog should open showing 3 duplicate pairs
5. Make decisions (skip/import)
6. After closing dialog, "Finalize Import" button should appear

## Technical Details

**Component State:**
- `hasDuplicates()` - Computed signal from `previewData()?.duplicates?.length > 0`
- `duplicateDecisions()` - Signal storing Map of user decisions (skip/import)
- `checkForDuplicates()` - Opens `DuplicateWarningDialogComponent`

**Key Condition:**
```typescript
hasDuplicates() && duplicateDecisions().size === 0
```
This condition is TRUE when backend found duplicates but user hasn't reviewed them yet.

## Files Changed

- `src/Ledgerly.Web/src/app/features/import/import-csv.component.html` (lines 254-263)

## Status

✅ **FIXED** - Button now appears when duplicates are detected
