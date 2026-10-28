using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pausalio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKpoEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KpoEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    BusinessProfileId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Year = table.Column<int>(type: "int", nullable: false),
                    SequenceNumber = table.Column<int>(type: "int", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    ServiceIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    TotalIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    OriginalCurrency = table.Column<int>(type: "int", nullable: false),
                    OriginalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    ExchangeRate = table.Column<decimal>(type: "decimal(18,4)", nullable: false, defaultValue: 1m),
                    EntryType = table.Column<int>(type: "int", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    OriginalEntryId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KpoEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KpoEntries_BusinessProfiles_BusinessProfileId",
                        column: x => x.BusinessProfileId,
                        principalTable: "BusinessProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KpoEntries_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_KpoEntries_KpoEntries_OriginalEntryId",
                        column: x => x.OriginalEntryId,
                        principalTable: "KpoEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_KpoEntries_Business_Invoice",
                table: "KpoEntries",
                columns: new[] { "BusinessProfileId", "InvoiceId" });

            migrationBuilder.CreateIndex(
                name: "IX_KpoEntries_Business_Year",
                table: "KpoEntries",
                columns: new[] { "BusinessProfileId", "Year" });

            migrationBuilder.CreateIndex(
                name: "IX_KpoEntries_Business_Year_Seq",
                table: "KpoEntries",
                columns: new[] { "BusinessProfileId", "Year", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_KpoEntries_InvoiceId",
                table: "KpoEntries",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_KpoEntries_OriginalEntryId",
                table: "KpoEntries",
                column: "OriginalEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KpoEntries");
        }
    }
}
