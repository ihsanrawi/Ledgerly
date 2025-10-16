using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ledgerly.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnMappingRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ColumnMappingRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    BankIdentifier = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    BankMatchPattern = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    HeaderSignature = table.Column<string>(type: "TEXT", nullable: false),
                    ColumnMappings = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimesUsed = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColumnMappingRules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ColumnMappingRules_BankIdentifier",
                table: "ColumnMappingRules",
                column: "BankIdentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ColumnMappingRules");
        }
    }
}
