using Microsoft.EntityFrameworkCore;

namespace Ledgerly.Api.Common.Data;

/// <summary>
/// Entity Framework DbContext for Ledgerly.
/// CRITICAL: SQLite is used for CACHING ONLY - .hledger files are the source of truth.
/// This database stores:
/// - Query result caches (dashboard data, reports)
/// - Import rules (category suggestions)
/// - Recurring transaction patterns
/// - Cache metadata (invalidation timestamps)
///
/// NEVER store authoritative transaction data in SQLite.
/// </summary>
public class LedgerlyDbContext : DbContext
{
    public LedgerlyDbContext(DbContextOptions<LedgerlyDbContext> options)
        : base(options)
    {
    }

    // DbSets will be added in future stories as needed

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Model configuration will be added in future stories
    }
}
