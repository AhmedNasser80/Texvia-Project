using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Texvia.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddiingISRevokeProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRevoked",
                table: "UserRefreshTokens",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRevoked",
                table: "UserRefreshTokens");
        }
    }
}
