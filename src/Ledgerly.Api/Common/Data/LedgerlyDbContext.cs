using Ledgerly.Api.Common.Data.Entities;
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

    /// <summary>
    /// Column mapping rules for CSV imports (Story 2.4).
    /// </summary>
    public DbSet<ColumnMappingRule> ColumnMappingRules => Set<ColumnMappingRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Model configuration will be added in future stories

        // Story 2.4: ColumnMappingRule configuration
        modelBuilder.Entity<ColumnMappingRule>(entity =>
        {
            entity.ToTable("ColumnMappingRules");
            entity.HasKey(e => e.Id);

            // Index on BankIdentifier for fast lookup
            entity.HasIndex(e => e.BankIdentifier);

            // Row version for optimistic concurrency
            entity.Property(e => e.RowVersion).IsRowVersion();
        });
    }
}
