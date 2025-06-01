using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WordWisp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDictionariesAndWords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dictionaries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dictionaries", x => x.id);
                    table.ForeignKey(
                        name: "FK_Dictionaries_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Words",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    term = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    definition = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    transcription = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    example = table.Column<string>(type: "text", nullable: true),
                    dictionary_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Words", x => x.id);
                    table.ForeignKey(
                        name: "FK_Words_Dictionaries_dictionary_id",
                        column: x => x.dictionary_id,
                        principalTable: "Dictionaries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dictionaries_user_id",
                table: "Dictionaries",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_Words_dictionary_id",
                table: "Words",
                column: "dictionary_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Words");

            migrationBuilder.DropTable(
                name: "Dictionaries");
        }
    }
}
