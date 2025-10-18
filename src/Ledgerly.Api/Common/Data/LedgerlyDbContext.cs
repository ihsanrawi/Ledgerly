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

    /// <summary>
    /// Import rules for category suggestions (Story 2.5).
    /// </summary>
    public DbSet<ImportRule> ImportRules => Set<ImportRule>();

    /// <summary>
    /// Transactions cache for duplicate detection (Story 2.5).
    /// CRITICAL: This is a CACHE only - .hledger files are source of truth.
    /// </summary>
    public DbSet<Transaction> Transactions => Set<Transaction>();

    /// <summary>
    /// CSV import audit records (Story 2.6).
    /// </summary>
    public DbSet<CsvImport> CsvImports => Set<CsvImport>();

    /// <summary>
    /// Hledger file modification audit trail (Story 2.6).
    /// </summary>
    public DbSet<HledgerFileAudit> HledgerFileAudits => Set<HledgerFileAudit>();

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

        // Story 2.5: ImportRule configuration
        modelBuilder.Entity<ImportRule>(entity =>
        {
            entity.ToTable("ImportRules");
            entity.HasKey(e => e.Id);

            // Index on Priority for fast rule ordering
            entity.HasIndex(e => e.Priority);

            entity.Property(e => e.PayeePattern).HasMaxLength(200).IsRequired();
            entity.Property(e => e.SuggestedCategory).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Confidence).HasPrecision(5, 4); // 0.0000 to 1.0000
        });

        // Story 2.5: Transaction configuration for duplicate detection
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transactions");
            entity.HasKey(e => e.HledgerTransactionCode);

            // Index on Hash for O(1) duplicate detection
            entity.HasIndex(e => e.Hash);

            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Payee).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Account).HasMaxLength(200).IsRequired();
            entity.Property(e => e.CategoryAccount).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Hash).HasMaxLength(44).IsRequired(); // Base64 SHA256 = 44 chars
        });

        // Story 2.6: CsvImport audit configuration
        modelBuilder.Entity<CsvImport>(entity =>
        {
            entity.ToTable("CsvImports");
            entity.HasKey(e => e.Id);

            // Index on ImportedAt for chronological queries
            entity.HasIndex(e => e.ImportedAt);

            // Index on UserId for user-specific import history
            entity.HasIndex(e => e.UserId);

            entity.Property(e => e.FileName).HasMaxLength(255).IsRequired();
            entity.Property(e => e.FileHash).HasMaxLength(64).IsRequired(); // SHA256 hex = 64 chars
        });

        // Story 2.6: HledgerFileAudit configuration
        modelBuilder.Entity<HledgerFileAudit>(entity =>
        {
            entity.ToTable("HledgerFileAudits");
            entity.HasKey(e => e.Id);

            // Index on Timestamp for chronological queries
            entity.HasIndex(e => e.Timestamp);

            // Index on Operation for filtering by operation type
            entity.HasIndex(e => e.Operation);

            // Index on UserId for user-specific audit trail
            entity.HasIndex(e => e.UserId);

            entity.Property(e => e.FileHashBefore).HasMaxLength(64).IsRequired();
            entity.Property(e => e.FileHashAfter).HasMaxLength(64).IsRequired();
            entity.Property(e => e.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(e => e.BalanceChecksum).HasPrecision(18, 2);
        });
    }
}
