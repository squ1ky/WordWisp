using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WordWisp.API.Migrations
{
    /// <inheritdoc />
    public partial class LevelTestTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "level_test_questions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    section = table.Column<int>(type: "integer", nullable: false),
                    question_text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    reading_passage = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    option_a = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    option_b = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    option_c = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    option_d = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    correct_answer = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    difficulty = table.Column<int>(type: "integer", nullable: false),
                    order_in_section = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_level_test_questions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "level_tests",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    total_questions = table.Column<int>(type: "integer", nullable: false),
                    time_limit_minutes = table.Column<int>(type: "integer", nullable: false),
                    grammar_score = table.Column<int>(type: "integer", nullable: true),
                    vocabulary_score = table.Column<int>(type: "integer", nullable: true),
                    reading_score = table.Column<int>(type: "integer", nullable: true),
                    total_score = table.Column<int>(type: "integer", nullable: true),
                    determined_level = table.Column<int>(type: "integer", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_level_tests", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "level_test_answers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    level_test_id = table.Column<int>(type: "integer", nullable: false),
                    question_id = table.Column<int>(type: "integer", nullable: false),
                    selected_answer = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    is_correct = table.Column<bool>(type: "boolean", nullable: false),
                    answered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_level_test_answers", x => x.id);
                    table.ForeignKey(
                        name: "FK_level_test_answers_level_test_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "level_test_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_level_test_answers_level_tests_level_test_id",
                        column: x => x.level_test_id,
                        principalTable: "level_tests",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_level_test_answers_level_test_id",
                table: "level_test_answers",
                column: "level_test_id");

            migrationBuilder.CreateIndex(
                name: "IX_level_test_answers_level_test_id_question_id",
                table: "level_test_answers",
                columns: new[] { "level_test_id", "question_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_level_test_answers_question_id",
                table: "level_test_answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_level_test_questions_section",
                table: "level_test_questions",
                column: "section");

            migrationBuilder.CreateIndex(
                name: "IX_level_test_questions_section_is_active",
                table: "level_test_questions",
                columns: new[] { "section", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_level_tests_user_id",
                table: "level_tests",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_level_tests_user_id_status",
                table: "level_tests",
                columns: new[] { "user_id", "status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "level_test_answers");

            migrationBuilder.DropTable(
                name: "level_test_questions");

            migrationBuilder.DropTable(
                name: "level_tests");
        }
    }
}
