using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordWisp.API.Migrations
{
    /// <inheritdoc />
    public partial class LevelTestAnswerModified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "estimated_user_level",
                table: "level_test_answers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "question_difficulty",
                table: "level_test_answers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "question_order",
                table: "level_test_answers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_level_test_answers_level_test_id_question_order",
                table: "level_test_answers",
                columns: new[] { "level_test_id", "question_order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_level_test_answers_level_test_id_question_order",
                table: "level_test_answers");

            migrationBuilder.DropColumn(
                name: "estimated_user_level",
                table: "level_test_answers");

            migrationBuilder.DropColumn(
                name: "question_difficulty",
                table: "level_test_answers");

            migrationBuilder.DropColumn(
                name: "question_order",
                table: "level_test_answers");
        }
    }
}
