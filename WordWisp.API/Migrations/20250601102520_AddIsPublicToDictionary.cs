using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordWisp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPublicToDictionary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_public",
                table: "Dictionaries",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_public",
                table: "Dictionaries");
        }
    }
}
