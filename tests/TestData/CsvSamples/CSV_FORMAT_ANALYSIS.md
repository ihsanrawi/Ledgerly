# CSV Format Analysis

**Purpose:** Document all CSV format variations found in collected bank statement samples to inform CSV parser development.

**Date:** 2025-10-08
**Samples Analyzed:** 12 CSV files
**Story:** 2.1 - Collect Bank CSV Test Samples

---

## 1. Delimiter Variations

| Delimiter | Frequency | Example Banks | Notes |
|-----------|-----------|---------------|-------|
| Comma (`,`) | 83% (10/12) | Chase, Bank of America, Wells Fargo, Citi | Most common in US banks |
| Semicolon (`;`) | 17% (2/12) | European banks | Common in Germany, France, Spain |
| Tab (`\t`) | 0% (0/12) | None found | Rare but possible |
| Pipe (`\|`) | 0% (0/12) | None found | Very rare |

**Parser Implication:** Auto-detect delimiter by analyzing first few rows. CsvHelper supports `CsvConfiguration.Delimiter` auto-detection.

---

## 2. Date Format Variations

| Format | Frequency | Example | Banks Using |
|--------|-----------|---------|-------------|
| `MM/DD/YYYY` | 50% (6/12) | `01/15/2025` | Chase, Bank of America, Citi |
| `YYYY-MM-DD` | 33% (4/12) | `2025-01-15` | Wells Fargo, Generic samples |
| `DD.MM.YYYY` | 17% (2/12) | `15.01.2025` | European banks |
| `M/D/YYYY` | Variant | `1/5/2025` | Chase (non-zero-padded) |
| `MMM DD, YYYY` | 0% | `Jan 15, 2025` | Not found in samples (but possible) |

**Parser Implication:** Use Luxon with multiple format patterns. Try formats in order of frequency. ISO format (`YYYY-MM-DD`) should parse first for performance.

**Date Column Names:**
- "Date" (most common)
- "Transaction Date"
- "Post Date"
- "Effective Date"
- "Datum" (German)

---

## 3. Encoding Variations

| Encoding | Frequency | Notes |
|----------|-----------|-------|
| UTF-8 | 100% (12/12) | All collected samples |
| ISO-8859-1 | 0% (0/12) | Not found but common in older systems |
| Windows-1252 | 0% (0/12) | Not found but possible |

**Special Character Handling:**
- ✅ Accented characters: `é`, `ñ`, `ö`, `ü`, `ç` (found in payee names)
- ✅ Emoji: `🌮`, `🍕` (found in 1 sample - special-characters.csv)
- ✅ Ampersand: `&` (found in business names)
- ✅ Apostrophes: `'` (found in names like "José's")

**Parser Implication:** CsvHelper with UTF-8 default should handle most cases. Add fallback detection for ISO-8859-1 if parsing fails.

---

## 4. Header Row Variations

| Header Pattern | Frequency | Notes |
|----------------|-----------|-------|
| Single header row | 92% (11/12) | Standard pattern |
| No header row | 8% (1/12) | Rare but must handle (no-headers.csv) |
| Multiple header rows | 0% (0/12) | Not found but documented as possible |
| Metadata rows before headers | 0% (0/12) | Not found (e.g., "Account: 123456" above headers) |

**Parser Implication:**
- Default: Assume row 1 is header
- Fallback: If row 1 contains data patterns (dates, amounts), assume no headers and infer column types
- Future: Add metadata row detection if encountered

---

## 5. Column Name Patterns

### Date Columns
- "Date" (50%)
- "Transaction Date" (25%)
- "Post Date" (17%)
- "Datum" (8% - German)
- Variations: "Effective Date", "Value Date"

### Description/Payee Columns
- "Description" (67%)
- "Payee" (8%)
- "Merchant" (8%)
- "Beschreibung" (8% - German)
- Variations: "Memo", "Details", "Transaction Description"

### Amount Columns

**Single Amount Column:**
- "Amount" (50%)
- "Betrag" (8% - German)
- Pattern: Negative = expense, Positive = income

**Split Debit/Credit Columns:**
- "Debit" / "Credit" (42%)
- "Withdrawal" / "Deposit" (rare)
- Pattern: Only one column populated per row

### Balance Columns
- "Balance" (42%)
- "Running Bal." (25%)
- "Saldo" (8% - German)
- "Available Balance" (not found but common)
- "Current Balance" (not found but common)

**Parser Implication:** Create fuzzy matching algorithm:
```
dateColumnNames = ["date", "transaction date", "post date", "effective date", "datum"]
descriptionColumnNames = ["description", "payee", "merchant", "memo", "details", "beschreibung"]
amountColumnNames = ["amount", "betrag"]
debitColumnNames = ["debit", "withdrawal", "outflow", "payment"]
creditColumnNames = ["credit", "deposit", "inflow", "receipt"]
balanceColumnNames = ["balance", "running bal", "saldo", "available balance"]
```

Match with case-insensitive substring matching.

---

## 6. Amount Representations

### Negative Amount Patterns

| Pattern | Frequency | Example | Banks |
|---------|-----------|---------|-------|
| `-` prefix | 75% (9/12) | `-50.00` | Chase, Wells Fargo, most US banks |
| `()` parentheses | 25% (3/12) | `(50.00)` | Citi, some credit cards |
| `DR` suffix | 0% (0/12) | `50.00 DR` | Not found (but documented as possible) |

**Parser Implication:**
1. Check for `()` pattern first: `^\((\d+\.?\d*)\)$`
2. Check for `-` prefix: `^-(\d+\.?\d*)$`
3. Check for `DR` suffix: `^(\d+\.?\d*)\s*DR$`

### Decimal Separator Patterns

| Pattern | Frequency | Example | Region |
|---------|-----------|---------|--------|
| `.` (period) | 83% (10/12) | `1234.56` | US, UK |
| `,` (comma) | 17% (2/12) | `1234,56` | Europe |

**Thousands Separator:**
- US: `1,234.56` (comma thousands, period decimal)
- Europe: `1.234,56` (period thousands, comma decimal)

**Parser Implication:** Detect decimal separator by analyzing number patterns. If `,` appears once at end → European format. If `.` appears once at end → US format.

### Currency Symbols

| Symbol | Frequency | Placement | Example |
|--------|-----------|-----------|---------|
| None | 92% (11/12) | N/A | `50.00` |
| `$` | 8% (1/12) | Prefix | `$50.00` |
| `USD` | 0% | Prefix or suffix | `USD 50.00` or `50.00 USD` |
| `€` | 0% | Prefix or suffix | `€50,00` or `50,00€` |

**Parser Implication:** Strip currency symbols before parsing: `Regex.Replace(amount, @"[^\d.,-()]", "")`

---

## 7. Edge Cases and Parser Challenges

### Multi-Line Memos (Found: 1 sample)

**Example:**
```csv
Date,Payee,Amount,Memo
2025-01-15,ACME Corp,1500.00,"Invoice #12345
Payment for consulting services
January 2025"
```

**Challenge:** Newlines inside quoted fields break naive line-by-line parsing.

**Solution:** CsvHelper handles this with `CsvConfiguration.IgnoreQuotes = false` (default).

---

### Special Characters (Found: 1 sample)

**Examples:**
- Accented characters: `Café René`, `José's Taquería`
- Emoji: `🌮 Taco Place`, `🍕 Pizza Shop`
- Ampersand: `Smith & Sons`

**Challenge:** Non-ASCII characters may cause encoding issues.

**Solution:** Ensure UTF-8 encoding. Test with `Encoding.UTF8.GetString()`.

---

### Empty Columns (Found: 2 samples)

**Example:**
```csv
Date,Description,Debit,Credit,Balance
2025-01-15,Purchase,50.00,,1234.56
2025-01-12,Payment,,100.00,1284.56
```

**Challenge:** Empty values in Debit or Credit columns.

**Solution:** CsvHelper parses empty strings as `""`. Convert to `null` or `0` in mapper.

---

### Blank Rows (Found: 0 samples, but documented as possible)

**Example:**
```csv
Date,Description,Amount

2025-01-15,Purchase,-50.00

2025-01-12,Payment,100.00
```

**Challenge:** Blank rows between transactions.

**Solution:** Filter out rows where all fields are empty: `row.All(field => string.IsNullOrWhiteSpace(field))`.

---

### Summary/Total Rows (Found: 0 samples, but documented as possible)

**Example:**
```csv
Date,Description,Amount,Balance
2025-01-15,Purchase,-50.00,1234.56
2025-01-12,Payment,100.00,1284.56
TOTAL,,50.00,
```

**Challenge:** Non-transaction summary rows at end of file.

**Solution:** Detect by checking Date column for non-date values. Skip rows where date parsing fails.

---

### Multiple Header Rows (Found: 0 samples, but documented as possible)

**Example:**
```csv
Account Number: 123456789
Statement Period: 01/01/2025 - 01/31/2025

Date,Description,Amount,Balance
2025-01-15,Purchase,-50.00,1234.56
```

**Challenge:** Metadata rows before actual headers.

**Solution:** Skip rows until valid header row detected (contains "Date" or "Amount").

---

### No Header Row (Found: 1 sample)

**Example:**
```csv
2025-01-20,Coffee Shop,-5.75,1250.00
2025-01-18,Gas Station,-45.00,1255.75
```

**Challenge:** Must infer column types from data patterns.

**Solution:**
1. Analyze first row
2. Detect date pattern → Date column
3. Detect number pattern → Amount/Balance columns
4. String without number/date → Description column

---

### Transaction Splits (Found: 0 samples, not supported in Story 2.1)

**Example:**
```csv
Date,Description,Amount,Category
2025-01-15,Walmart,-100.00,
,,,-60.00,Groceries
,,,-40.00,Household
```

**Challenge:** Single transaction spanning multiple rows.

**Status:** ❌ **NOT SUPPORTED IN MVP** - Defer to Phase 2. Document as known limitation.

---

## 8. Recommended Auto-Detection Algorithm

Based on analysis of 12 samples, here's the recommended column detection approach:

### Step 1: Detect Delimiter
```csharp
var delimiters = new[] { ',', ';', '\t', '|' };
foreach (var delimiter in delimiters)
{
    var firstRow = file.ReadFirstLine().Split(delimiter);
    if (firstRow.Length >= 3) // Minimum 3 columns expected
    {
        detectedDelimiter = delimiter;
        break;
    }
}
```

### Step 2: Detect Headers
```csharp
var firstRow = csvReader.ReadFirstRow();
bool hasHeaders = firstRow.Any(cell =>
    cell.ToLower().Contains("date") ||
    cell.ToLower().Contains("description") ||
    cell.ToLower().Contains("amount"));
```

### Step 3: Map Columns
```csharp
var dateColumn = FindColumn(headers, ["date", "transaction date", "post date", "datum"]);
var descColumn = FindColumn(headers, ["description", "payee", "merchant", "beschreibung"]);
var amountColumn = FindColumn(headers, ["amount", "betrag"]);
var debitColumn = FindColumn(headers, ["debit", "withdrawal"]);
var creditColumn = FindColumn(headers, ["credit", "deposit"]);
var balanceColumn = FindColumn(headers, ["balance", "running bal", "saldo"]);

string FindColumn(string[] headers, string[] patterns)
{
    return headers.FirstOrDefault(h =>
        patterns.Any(p => h.ToLower().Contains(p)));
}
```

### Step 4: Parse Amounts
```csharp
decimal ParseAmount(string amountStr)
{
    // Remove currency symbols
    amountStr = Regex.Replace(amountStr, @"[^\d.,-()]", "");

    // Handle parentheses (negative)
    if (amountStr.StartsWith("(") && amountStr.EndsWith(")"))
    {
        amountStr = "-" + amountStr.Trim('(', ')');
    }

    // Detect decimal separator
    bool isEuropean = amountStr.Contains(",") && !amountStr.Contains(".");
    if (isEuropean)
    {
        amountStr = amountStr.Replace(".", "").Replace(",", ".");
    }

    return decimal.Parse(amountStr);
}
```

---

## Summary Statistics

**Total Samples Collected:** 12
**Standard Formats:** 4 (33%)
**Edge Cases:** 4 (33%)
**Malformed:** 4 (33%)

**Delimiter Distribution:**
- Comma: 83%
- Semicolon: 17%

**Date Format Distribution:**
- MM/DD/YYYY: 50%
- YYYY-MM-DD: 33%
- DD.MM.YYYY: 17%

**Negative Amount Representation:**
- Minus sign: 75%
- Parentheses: 25%

**Encoding:**
- UTF-8: 100%

**Header Row:**
- Present: 92%
- Absent: 8%

---

## Next Steps (Story 2.2+)

1. **Implement column auto-detection algorithm** using patterns documented above
2. **Test parser against all 12 samples** to validate ≥90% auto-detection success rate
3. **Build manual mapping UI** for cases where auto-detection fails
4. **Add validation** for date/amount parsing with helpful error messages

---

## References

- [CsvHelper Documentation](https://joshclose.github.io/CsvHelper/)
- [ISO 8601 Date Format](https://en.wikipedia.org/wiki/ISO_8601)
- Story 2.1 Acceptance Criteria (docs/prd/epic-details.md)
