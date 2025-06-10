using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordWisp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUuidOsspExtension : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
