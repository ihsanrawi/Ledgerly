using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ledgerly.Api.Common.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCsvImportAndFileAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CsvImports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ImportedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TotalRows = table.Column<int>(type: "INTEGER", nullable: false),
                    SuccessfulImports = table.Column<int>(type: "INTEGER", nullable: false),
                    DuplicatesSkipped = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorCount = table.Column<int>(type: "INTEGER", nullable: false),
                    BankFormat = table.Column<string>(type: "TEXT", nullable: true),
                    ColumnMapping = table.Column<string>(type: "TEXT", nullable: true),
                    FileHash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CsvImports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HledgerFileAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Operation = table.Column<int>(type: "INTEGER", nullable: false),
                    FileHashBefore = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    FileHashAfter = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    TransactionCount = table.Column<int>(type: "INTEGER", nullable: false),
                    BalanceChecksum = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    TriggeredBy = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true),
                    RelatedEntityId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FilePath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HledgerFileAudits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CsvImports_ImportedAt",
                table: "CsvImports",
                column: "ImportedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CsvImports_UserId",
                table: "CsvImports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HledgerFileAudits_Operation",
                table: "HledgerFileAudits",
                column: "Operation");

            migrationBuilder.CreateIndex(
                name: "IX_HledgerFileAudits_Timestamp",
                table: "HledgerFileAudits",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_HledgerFileAudits_UserId",
                table: "HledgerFileAudits",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CsvImports");

            migrationBuilder.DropTable(
                name: "HledgerFileAudits");
        }
    }
}
