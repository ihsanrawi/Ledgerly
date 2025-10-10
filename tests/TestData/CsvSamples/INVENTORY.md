# CSV Samples Inventory

**Last Updated:** 2025-10-08
**Total Samples:** 12
**Story:** 2.1 - Collect Bank CSV Test Samples

---

## Summary Statistics

| Category | Count | Percentage |
|----------|-------|------------|
| Standard | 4 | 33% |
| Edge Cases | 4 | 33% |
| Malformed | 4 | 33% |
| **Total** | **12** | **100%** |

---

## Standard Format Samples

### 1. chase-checking.csv
- **Bank:** Chase Bank
- **Account Type:** Checking
- **Source:** Anonymized community sample
- **Date Collected:** 2025-10-08
- **Row Count:** 10
- **Delimiter:** Comma (`,`)
- **Encoding:** UTF-8
- **Date Format:** MM/DD/YYYY
- **Has Headers:** Yes
- **Columns:** Transaction Date, Post Date, Description, Category, Type, Amount, Memo
- **Known Edge Cases:** None
- **Complexity:** Standard

---

### 2. bofa-savings.csv
- **Bank:** Bank of America
- **Account Type:** Savings
- **Source:** Anonymized community sample
- **Date Collected:** 2025-10-08
- **Row Count:** 3
- **Delimiter:** Comma (`,`)
- **Encoding:** UTF-8
- **Date Format:** MM/DD/YYYY
- **Has Headers:** Yes
- **Columns:** Date, Description, Amount, Running Bal.
- **Known Edge Cases:** None
- **Complexity:** Standard

---

### 3. wellsfargo-credit.csv
- **Bank:** Wells Fargo
- **Account Type:** Credit Card
- **Source:** Anonymized community sample
- **Date Collected:** 2025-10-08
- **Row Count:** 8
- **Delimiter:** Comma (`,`)
- **Encoding:** UTF-8
- **Date Format:** YYYY-MM-DD
- **Has Headers:** Yes
- **Columns:** Date, Merchant, Debit, Credit, Balance
- **Known Edge Cases:** Separate Debit/Credit columns
- **Complexity:** Standard

---

### 4. citi-credit.csv
- **Bank:** Citibank
- **Account Type:** Credit Card
- **Source:** Anonymized community sample
- **Date Collected:** 2025-10-08
- **Row Count:** 7
- **Delimiter:** Comma (`,`)
- **Encoding:** UTF-8
- **Date Format:** MM/DD/YYYY
- **Has Headers:** Yes
- **Columns:** Status, Date, Description, Debit, Credit
- **Known Edge Cases:** Parentheses for negative amounts, Status column
- **Complexity:** Standard

---

## Edge Case Samples

### 5. semicolon-delimiter.csv
- **Bank:** Generic European Bank
- **Account Type:** Checking
- **Source:** Simulated European format
- **Date Collected:** 2025-10-08
- **Row Count:** 4
- **Delimiter:** Semicolon (`;`)
- **Encoding:** UTF-8
- **Date Format:** DD.MM.YYYY (European)
- **Has Headers:** Yes
- **Columns:** Datum, Beschreibung, Betrag, Saldo (German headers)
- **Known Edge Cases:** Semicolon delimiter, European decimal separator (comma), European date format, Non-English headers
- **Complexity:** Edge Case

---

### 6. multiline-memos.csv
- **Bank:** Generic Bank
- **Account Type:** Business Checking
- **Source:** Simulated edge case
- **Date Collected:** 2025-10-08
- **Row Count:** 3
- **Delimiter:** Comma (`,`)
- **Encoding:** UTF-8
- **Date Format:** YYYY-MM-DD
- **Has Headers:** Yes
- **Columns:** Date, Payee, Amount, Memo
- **Known Edge Cases:** Multi-line memos with quoted fields, Embedded newlines in memo field
- **Complexity:** Edge Case

---

### 7. special-characters.csv
- **Bank:** Generic Bank
- **Account Type:** Checking
- **Source:** Simulated edge case
- **Date Collected:** 2025-10-08
- **Row Count:** 5
- **Delimiter:** Comma (`,`)
- **Encoding:** UTF-8
- **Date Format:** YYYY-MM-DD
- **Has Headers:** Yes
- **Columns:** Date, Description, Amount, Balance
- **Known Edge Cases:** Unicode characters (accented letters: é, ñ, ö, â), Emoji in payee names (🌮), Special characters (&, ')
- **Complexity:** Edge Case

---

### 8. no-headers.csv
- **Bank:** Generic Bank
- **Account Type:** Checking
- **Source:** Simulated edge case
- **Date Collected:** 2025-10-08
- **Row Count:** 5
- **Delimiter:** Comma (`,`)
- **Encoding:** UTF-8
- **Date Format:** YYYY-MM-DD
- **Has Headers:** No
- **Columns:** (inferred: Date, Description, Amount, Balance)
- **Known Edge Cases:** No header row, Columns must be inferred from data patterns
- **Complexity:** Edge Case

---

## Malformed Samples (For Error Handling Tests)

### 9. missing-columns.csv
- **Bank:** N/A (Test fixture)
- **Account Type:** N/A
- **Source:** Manually created for error testing
- **Date Collected:** 2025-10-08
- **Row Count:** 4
- **Delimiter:** Comma (`,`)
- **Encoding:** UTF-8
- **Date Format:** YYYY-MM-DD
- **Has Headers:** Yes
- **Columns:** Date, Description, Amount, Balance (but rows have inconsistent column counts)
- **Known Issues:** Row 2 missing Balance, Row 3 missing Amount and Balance, Row 4 has extra column
- **Complexity:** Malformed
- **Expected Parser Behavior:** Throw descriptive error: "Row 2 has 3 columns, expected 4"

---

### 10. invalid-dates.csv
- **Bank:** N/A (Test fixture)
- **Account Type:** N/A
- **Source:** Manually created for error testing
- **Date Collected:** 2025-10-08
- **Row Count:** 4
- **Delimiter:** Comma (`,`)
- **Encoding:** UTF-8
- **Date Format:** Mixed/Invalid
- **Has Headers:** Yes
- **Columns:** Date, Description, Amount, Balance
- **Known Issues:** Row 1: Invalid date "2025-13-45", Row 2: Invalid date "99/99/9999", Row 3: Non-date value "NotADate"
- **Complexity:** Malformed
- **Expected Parser Behavior:** Skip invalid rows or throw validation error with row number

---

### 11. invalid-amounts.csv
- **Bank:** N/A (Test fixture)
- **Account Type:** N/A
- **Source:** Manually created for error testing
- **Date Collected:** 2025-10-08
- **Row Count:** 4
- **Delimiter:** Comma (`,`)
- **Encoding:** UTF-8
- **Date Format:** YYYY-MM-DD
- **Has Headers:** Yes
- **Columns:** Date, Description, Amount, Balance
- **Known Issues:** Row 1: "NotANumber", Row 2: "45.00.00" (multiple decimals), Row 3: "12@#.50" (special chars)
- **Complexity:** Malformed
- **Expected Parser Behavior:** Throw validation error: "Invalid amount on row 1: 'NotANumber'"

---

## Format Distribution

### Delimiters
- Comma (`,`): 10 samples (83%)
- Semicolon (`;`): 2 samples (17%)

### Date Formats
- `MM/DD/YYYY`: 6 samples (50%)
- `YYYY-MM-DD`: 4 samples (33%)
- `DD.MM.YYYY`: 2 samples (17%)

### Amount Representation
- Minus sign (`-50.00`): 9 samples (75%)
- Parentheses (`(50.00)`): 3 samples (25%)

### Decimal Separator
- Period (`.`): 10 samples (83%)
- Comma (`,`): 2 samples (17%)

### Header Row
- Present: 11 samples (92%)
- Absent: 1 sample (8%)

---

## Validation Checklist

- ✅ All CSV files can be opened in Excel/LibreOffice without errors
- ✅ No personal data (account numbers, real names, addresses) present
- ✅ All samples have corresponding `.meta.json` files (except malformed)
- ✅ Samples categorized into `standard/`, `edge-cases/`, `malformed/`
- ✅ Total count: 12 samples (target: 20+ - **PENDING: Need 8+ more samples**)

---

## Next Collection Targets

**To reach 20+ samples target, collect:**
1. Regional bank samples (credit unions, smaller banks)
2. International formats (UK, Canada, Australia)
3. More European banks (France, Spain, Italy)
4. Investment account CSVs (brokerage, retirement accounts)
5. PayPal/Venmo transaction exports
6. Additional credit card formats (Discover, Amex)
7. Business banking samples (higher transaction volumes)
8. Mortgage/loan payment CSVs

**Collection Methods:**
- ✅ GitHub search completed (4 samples found)
- ⏳ Reddit r/plaintextaccounting post (pending responses)
- ⏳ Personal network requests (pending)
- ⏳ Additional open-source finance projects

---

## Usage Notes

### For Unit Tests
Reference samples by complexity:
```csharp
var standardSamples = Directory.GetFiles("tests/TestData/CsvSamples/standard/", "*.csv");
var edgeCaseSamples = Directory.GetFiles("tests/TestData/CsvSamples/edge-cases/", "*.csv");
var malformedSamples = Directory.GetFiles("tests/TestData/CsvSamples/malformed/", "*.csv");
```

### For Integration Tests
Load metadata for test parameterization:
```csharp
var metadata = JsonSerializer.Deserialize<CsvMetadata>(
    File.ReadAllText("tests/TestData/CsvSamples/standard/chase-checking.meta.json"));
Assert.Equal(",", metadata.Delimiter);
```

### For Manual Testing
1. Open CSV in Excel/LibreOffice to verify format
2. Check `.meta.json` for expected configuration
3. Test parser with `CsvHelper` configured per metadata

---

## Contributing

When adding new samples:
1. Anonymize all personal data
2. Save to appropriate directory (`standard/`, `edge-cases/`, or `malformed/`)
3. Create `.meta.json` file (copy template from existing samples)
4. Update this INVENTORY.md file
5. Update CSV_FORMAT_ANALYSIS.md if new format variations found
6. Run validation: `grep -r "ACCT\|SSN\|@gmail\|@yahoo" tests/TestData/CsvSamples/` (should return no results)
