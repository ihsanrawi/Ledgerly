# CSV Test Samples

This directory contains real-world bank CSV samples collected for testing the CSV import feature in Ledgerly.

## Purpose

These samples are used to:
- Test CSV parser against diverse real-world formats
- Validate column auto-detection algorithms
- Test edge case handling (multi-line memos, special characters, etc.)
- Ensure error handling for malformed CSVs

## Directory Structure

```
CsvSamples/
├── README.md                      # This file
├── CSV_FORMAT_ANALYSIS.md         # Detailed analysis of format variations
├── INVENTORY.md                   # Complete list of all samples
├── column-name-mapping.json       # Reference data for auto-detection algorithm
├── standard/                      # Standard CSV formats (4-6 columns, headers present)
│   ├── {bank}-{type}.csv          # Anonymized bank CSV samples
│   └── {bank}-{type}.meta.json    # Metadata for each sample
├── edge-cases/                    # Unusual but valid formats
│   ├── multiline-memos.csv
│   ├── special-characters.csv
│   └── no-headers.csv
└── malformed/                     # Intentionally broken CSVs for error handling
    ├── missing-columns.csv
    └── invalid-encoding.csv
```

## File Naming Convention

- **CSV files:** `{bank-name}-{account-type}.csv` (lowercase, hyphen-separated)
  - Examples: `chase-checking.csv`, `bofa-savings.csv`, `wellsfargo-credit.csv`
- **Metadata files:** `{bank-name}-{account-type}.meta.json`
- **Documentation:** UPPERCASE.md for top-level docs

## Anonymization

**ALL CSV samples have been anonymized:**
- Account numbers replaced with `ACCT000001`, `ACCT000002`, etc.
- Real names replaced with `Test User`, `Jane Doe`, etc.
- Personal information removed (addresses, phone numbers, emails)
- Amounts kept realistic for testing purposes

## Sample Metadata Format

Each CSV in `standard/` and `edge-cases/` has a corresponding `.meta.json` file:

```json
{
  "bank": "Chase Bank",
  "accountType": "Checking",
  "source": "r/plaintextaccounting community",
  "dateCollected": "2025-10-08",
  "rowCount": 127,
  "complexityLevel": "standard",
  "knownEdgeCases": [],
  "delimiter": ",",
  "encoding": "UTF-8",
  "dateFormat": "MM/DD/YYYY",
  "hasHeaders": true
}
```

## Usage in Tests

These samples are referenced in:
- `src/Ledgerly.Api/Features/ImportCsv/ImportCsv.Tests/` - Unit tests
- `tests/Integration.Tests/CsvImportIntegrationTests.cs` - Integration tests

## Contributing

When adding new CSV samples:
1. Anonymize all personal data
2. Add the CSV to appropriate subdirectory
3. Create corresponding `.meta.json` file
4. Update `INVENTORY.md`
5. Document any new format variations in `CSV_FORMAT_ANALYSIS.md`
