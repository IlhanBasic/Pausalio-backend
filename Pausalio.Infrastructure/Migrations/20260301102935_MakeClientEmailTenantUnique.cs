using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pausalio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeClientEmailTenantUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clients_Email",
                table: "Clients");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Email_BusinessProfileId",
                table: "Clients",
                columns: new[] { "Email", "BusinessProfileId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clients_Email_BusinessProfileId",
                table: "Clients");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Email",
                table: "Clients",
                column: "Email",
                unique: true);
        }
    }
}
