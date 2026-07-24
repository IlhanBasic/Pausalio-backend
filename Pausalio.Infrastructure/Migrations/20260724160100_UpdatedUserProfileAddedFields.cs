using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pausalio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedUserProfileAddedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OpenRouterApiKey",
                table: "UserProfiles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "OpenRouterModelName",
                table: "UserProfiles",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OpenRouterApiKey",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "OpenRouterModelName",
                table: "UserProfiles");
        }
    }
}
