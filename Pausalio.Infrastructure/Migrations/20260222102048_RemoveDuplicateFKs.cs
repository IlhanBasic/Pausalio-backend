using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pausalio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDuplicateFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Payments_PaymentId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_TaxObligations_Payments_PaymentId",
                table: "TaxObligations");

            migrationBuilder.DropIndex(
                name: "IX_TaxObligations_PaymentId",
                table: "TaxObligations");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_PaymentId",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "TaxObligations");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "Expenses");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_TaxObligations_TaxObligationId",
                table: "Payments",
                column: "TaxObligationId",
                principalTable: "TaxObligations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_TaxObligations_TaxObligationId",
                table: "Payments");

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentId",
                table: "TaxObligations",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentId",
                table: "Expenses",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_TaxObligations_PaymentId",
                table: "TaxObligations",
                column: "PaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_PaymentId",
                table: "Expenses",
                column: "PaymentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Payments_PaymentId",
                table: "Expenses",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TaxObligations_Payments_PaymentId",
                table: "TaxObligations",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
