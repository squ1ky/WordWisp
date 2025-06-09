using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WordWisp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddReadingPassageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reading_passage",
                table: "level_test_questions");

            migrationBuilder.AddColumn<int>(
                name: "reading_passage_id",
                table: "level_test_questions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "reading_passages",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    topic = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    word_count = table.Column<int>(type: "integer", nullable: false),
                    estimated_reading_time = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reading_passages", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_level_test_questions_reading_passage_id",
                table: "level_test_questions",
                column: "reading_passage_id");

            migrationBuilder.CreateIndex(
                name: "IX_reading_passages_level",
                table: "reading_passages",
                column: "level");

            migrationBuilder.CreateIndex(
                name: "IX_reading_passages_level_is_active",
                table: "reading_passages",
                columns: new[] { "level", "is_active" });

            migrationBuilder.AddForeignKey(
                name: "FK_level_test_questions_reading_passages_reading_passage_id",
                table: "level_test_questions",
                column: "reading_passage_id",
                principalTable: "reading_passages",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_level_test_questions_reading_passages_reading_passage_id",
                table: "level_test_questions");

            migrationBuilder.DropTable(
                name: "reading_passages");

            migrationBuilder.DropIndex(
                name: "IX_level_test_questions_reading_passage_id",
                table: "level_test_questions");

            migrationBuilder.DropColumn(
                name: "reading_passage_id",
                table: "level_test_questions");

            migrationBuilder.AddColumn<string>(
                name: "reading_passage",
                table: "level_test_questions",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }
    }
}
