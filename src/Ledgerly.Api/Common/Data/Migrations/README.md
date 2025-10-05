# Migrations

Entity Framework Core migrations will be added here as needed in future stories.

Currently, no tables are defined since the SQLite database is for **caching only**.

The `.hledger` plain text files are the authoritative source of truth for all financial data.

## Creating Migrations

When tables are added:

```bash
cd src/Ledgerly.Api
dotnet ef migrations add MigrationName --context LedgerlyDbContext
dotnet ef database update
```
