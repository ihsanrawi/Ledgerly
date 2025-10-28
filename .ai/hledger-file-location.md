# Where the hledger File is Saved

## Quick Answer

**Location:** `/home/ihsan/.ledgerly/ledger.hledger`

**General Pattern:** `$HOME/.ledgerly/ledger.hledger`

---

## Full Path Resolution

### Code Location
[ConfirmImportHandler.cs:197-212](../src/Ledgerly.Api/Features/ImportCsv/ConfirmImportHandler.cs#L197-L212)

```csharp
private Task<string> ResolveHledgerFilePathAsync(Guid userId)
{
    // For MVP, use default path. In future, this would query UserSettings table.
    var defaultPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".ledgerly",
        "ledger.hledger");

    if (!File.Exists(defaultPath))
    {
        _logger.LogError("Hledger file not found: {FilePath}", defaultPath);
        throw new FileNotFoundException($"Hledger file not found: {defaultPath}");
    }

    return Task.FromResult(defaultPath);
}
```

### Path Components

| Component | Value | Description |
|-----------|-------|-------------|
| **Base** | `Environment.SpecialFolder.UserProfile` | User's home directory |
| **Folder** | `.ledgerly` | Hidden directory (Unix convention) |
| **File** | `ledger.hledger` | hledger journal file |

### Platform-Specific Paths

| OS | Path |
|----|------|
| **Linux** | `/home/username/.ledgerly/ledger.hledger` |
| **macOS** | `/Users/username/.ledgerly/ledger.hledger` |
| **Windows** | `C:\Users\username\.ledgerly\ledger.hledger` |

---

## Current File Status (Your System)

```bash
$ ls -lah /home/ihsan/.ledgerly/
total 24K
drwxr-xr-x  2 ihsan ihsan 4.0K Oct 26 07:50 .
drwx------ 34 ihsan ihsan 4.0K Oct 26 07:32 ..
-rw-r--r--  1 ihsan ihsan 7.9K Oct 26 07:50 ledger.hledger      # Main file
-rw-r--r--  1 ihsan ihsan 6.6K Oct 22 22:16 ledger.hledger.bak  # Backup
```

**File Size:** 7.9 KB (250 lines)
**Last Modified:** Oct 26, 07:50 AM
**Backup Exists:** Yes (created Oct 22)

---

## Additional Files in Same Directory

| File | Purpose | Created When |
|------|---------|--------------|
| `ledger.hledger` | Main journal file | API startup or first import |
| `ledger.hledger.bak` | Automatic backup | Before each write operation |
| `ledger.hledger.tmp` | Temporary write file | During write (deleted after) |

---

## File Contents Preview

**First 20 lines:**
```hledger
account assets:checking
account Assets:Checking
account expenses:groceries
account expenses:shopping
account expenses:utilities

; Test ledger file for integration tests
account assets:checking
account expenses:groceries
account expenses:shopping
account expenses:utilities

2025-01-15 (ce18d6ad-75bd-4389-852b-58e835743f5a) Coffee Shop
  expenses:groceries                                $4.50
  Assets:Checking
```

**Last 10 lines:**
```hledger
2025-01-19 (b3b1657c-2df7-4927-b629-43a476254d8a) Shell Gas
  expenses:groceries                                $45.00
  Assets:Checking

2025-01-20 (c90c35e8-8f55-4f3a-a571-28907cf68503) Chipotle
  expenses:groceries                                $12.50
  Assets:Checking
```

---

## File Initialization

### When is the file created?

The file is **NOT automatically created** by the API. It must already exist before CSV imports.

**Current Behavior:**
1. User must manually create `~/.ledgerly/ledger.hledger` before first import
2. Or: Integration tests create it with seed data
3. If file doesn't exist, import throws `FileNotFoundException`

**Code Check:**
```csharp
if (!File.Exists(defaultPath))
{
    _logger.LogError("Hledger file not found: {FilePath}", defaultPath);
    throw new FileNotFoundException($"Hledger file not found: {defaultPath}");
}
```

### How to create the file manually

```bash
# Create directory
mkdir -p ~/.ledgerly

# Create empty journal file
touch ~/.ledgerly/ledger.hledger

# OR: Create with account declarations
cat > ~/.ledgerly/ledger.hledger << 'EOF'
account Assets:Checking
account Expenses:Groceries
account Expenses:Dining
account Income:Salary
EOF
```

---

## Future Enhancements (Per Code Comments)

### Multi-User Support
```csharp
// For MVP, use default path. In future, this would query UserSettings table.
```

**Future Implementation:**
- Query `UserSettings` table for custom file path per user
- Support multiple journal files per user
- Support user-configurable paths

**Example Future Logic:**
```csharp
// Future implementation
var userSettings = await _dbContext.UserSettings
    .Where(u => u.UserId == userId)
    .FirstOrDefaultAsync();

var hledgerPath = userSettings?.HledgerFilePath
    ?? GetDefaultPath(userId);
```

---

## SQLite Database Location

While the hledger file is at `~/.ledgerly/ledger.hledger`, the SQLite cache database is separate:

**Default Location:** `ledgerly-cache.db` (in working directory)

**Code:** [Program.cs:103-106](../src/Ledgerly.Api/Program.cs#L103-L106)
```csharp
var connectionString = builder.Configuration.GetConnectionString("LedgerlyCache")
    ?? "Data Source=ledgerly-cache.db";
options.UseSqlite(connectionString);
```

**Your System:**
```bash
$ find ~/Desktop/Ledgerly -name "*.db" -type f
/home/ihsan/Desktop/Ledgerly/src/Ledgerly.Api/ledgerly-cache.db
```

---

## Binary Location (hledger executable)

The hledger binary is extracted to a different location:

**Default:** `~/.local/share/Ledgerly/bin/hledger-linux-x64`

**Confirmed in logs:**
```
[07:28:38 INF] hledger binary ready at /home/ihsan/.local/share/Ledgerly/bin/hledger-linux-x64
```

---

## Directory Structure Summary

```
/home/ihsan/
├── .ledgerly/                          # hledger journal files
│   ├── ledger.hledger                  # Main journal (7.9KB)
│   ├── ledger.hledger.bak              # Automatic backup (6.6KB)
│   └── ledger.hledger.tmp              # Temp file (during writes, deleted after)
│
├── .local/share/Ledgerly/bin/          # hledger binary
│   └── hledger-linux-x64               # Extracted from embedded resources
│
└── Desktop/Ledgerly/                   # Project directory
    └── src/Ledgerly.Api/
        ├── ledgerly-cache.db           # SQLite cache
        └── logs/                       # Application logs
            └── ledgerly-*.log
```

---

## Security & Permissions

### File Permissions
```bash
-rw-r--r-- 1 ihsan ihsan 7.9K Oct 26 07:50 ledger.hledger
```

**Breakdown:**
- Owner (ihsan): Read + Write
- Group (ihsan): Read only
- Others: Read only

**Recommended for privacy:**
```bash
chmod 600 ~/.ledgerly/ledger.hledger  # Owner only
```

### Directory Permissions
```bash
drwxr-xr-x 2 ihsan ihsan 4.0K Oct 26 07:50 .ledgerly
```

**Recommended for privacy:**
```bash
chmod 700 ~/.ledgerly  # Owner only
```

---

## Configuration Override

### Via appsettings.json (Future)

Currently hardcoded, but could be configured:

```json
{
  "Hledger": {
    "DefaultFilePath": "/custom/path/to/ledger.hledger"
  }
}
```

### Via Environment Variable (Future)

```bash
export LEDGERLY_HLEDGER_PATH="/custom/path/to/ledger.hledger"
```

---

## Troubleshooting

### File Not Found Error

**Error Message:**
```
Hledger file not found: /home/ihsan/.ledgerly/ledger.hledger
```

**Solution:**
```bash
mkdir -p ~/.ledgerly
touch ~/.ledgerly/ledger.hledger
```

### Permission Denied

**Error:** Cannot write to file

**Solution:**
```bash
chmod 644 ~/.ledgerly/ledger.hledger
chmod 755 ~/.ledgerly
```

### Backup Not Updating

**Check:**
```bash
ls -lt ~/.ledgerly/
```

**Verify:** `.bak` file timestamp should update on each import

---

## Related Documentation

- **Write Workflow:** [hledger-write-workflow.md](hledger-write-workflow.md)
- **Duplicate Detection:** [fix-review-duplicates-button.md](fix-review-duplicates-button.md)
- **API Logs:** [api-logs.md](api-logs.md)

---

## Summary

✅ **Location:** `~/.ledgerly/ledger.hledger`
✅ **Current Size:** 7.9 KB (250 lines)
✅ **Backup:** Automatic `.bak` file created on each write
✅ **Format:** Standard hledger journal syntax
✅ **Permissions:** User read/write (644)
✅ **Portable:** Can be opened with any hledger tool
