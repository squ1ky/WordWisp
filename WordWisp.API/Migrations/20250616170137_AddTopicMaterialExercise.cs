using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WordWisp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTopicMaterialExercise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "topics",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    created_by = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_topics", x => x.id);
                    table.ForeignKey(
                        name: "FK_topics_users_created_by",
                        column: x => x.created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "materials",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    content = table.Column<string>(type: "text", nullable: true),
                    material_type = table.Column<int>(type: "integer", nullable: false),
                    file_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    external_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    file_size = table.Column<long>(type: "bigint", nullable: true),
                    mime_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    original_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    topic_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_materials", x => x.id);
                    table.ForeignKey(
                        name: "FK_materials_topics_topic_id",
                        column: x => x.topic_id,
                        principalTable: "topics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exercises",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    exercise_type = table.Column<string>(type: "text", nullable: false),
                    time_limit = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    max_attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    passing_score = table.Column<int>(type: "integer", nullable: false, defaultValue: 70),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    material_id = table.Column<int>(type: "integer", nullable: true),
                    topic_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercises", x => x.id);
                    table.ForeignKey(
                        name: "FK_exercises_materials_material_id",
                        column: x => x.material_id,
                        principalTable: "materials",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_exercises_topics_topic_id",
                        column: x => x.topic_id,
                        principalTable: "topics",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exercise_questions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    question = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    question_image_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    question_audio_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    points = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    exercise_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_questions", x => x.id);
                    table.ForeignKey(
                        name: "FK_exercise_questions_exercises_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_exercise_attempts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    exercise_id = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0m),
                    max_possible_score = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 100m),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_passed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    time_spent_seconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_exercise_attempts", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_exercise_attempts_exercises_exercise_id",
                        column: x => x.exercise_id,
                        principalTable: "exercises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_exercise_attempts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exercise_answers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    answer_text = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    answer_image_path = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_correct = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    question_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exercise_answers", x => x.id);
                    table.ForeignKey(
                        name: "FK_exercise_answers_exercise_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "exercise_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_exercise_answers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_attempt_id = table.Column<int>(type: "integer", nullable: false),
                    question_id = table.Column<int>(type: "integer", nullable: false),
                    answer_text = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    selected_answer_id = table.Column<int>(type: "integer", nullable: true),
                    is_correct = table.Column<bool>(type: "boolean", nullable: true),
                    points_earned = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    answered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_exercise_answers", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_exercise_answers_exercise_answers_selected_answer_id",
                        column: x => x.selected_answer_id,
                        principalTable: "exercise_answers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_user_exercise_answers_exercise_questions_question_id",
                        column: x => x.question_id,
                        principalTable: "exercise_questions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_exercise_answers_user_exercise_attempts_user_attempt_id",
                        column: x => x.user_attempt_id,
                        principalTable: "user_exercise_attempts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_exercise_answers_question_id",
                table: "exercise_answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_exercise_questions_exercise_id",
                table: "exercise_questions",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "IX_exercises_material_id",
                table: "exercises",
                column: "material_id");

            migrationBuilder.CreateIndex(
                name: "IX_exercises_topic_id",
                table: "exercises",
                column: "topic_id");

            migrationBuilder.CreateIndex(
                name: "IX_materials_topic_id",
                table: "materials",
                column: "topic_id");

            migrationBuilder.CreateIndex(
                name: "IX_topics_created_by",
                table: "topics",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_UserExerciseAnswer_UserAttempt",
                table: "user_exercise_answers",
                column: "user_attempt_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_exercise_answers_question_id",
                table: "user_exercise_answers",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_exercise_answers_selected_answer_id",
                table: "user_exercise_answers",
                column: "selected_answer_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_exercise_attempts_exercise_id",
                table: "user_exercise_attempts",
                column: "exercise_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_exercise_attempts_user_id",
                table: "user_exercise_attempts",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_exercise_answers");

            migrationBuilder.DropTable(
                name: "exercise_answers");

            migrationBuilder.DropTable(
                name: "user_exercise_attempts");

            migrationBuilder.DropTable(
                name: "exercise_questions");

            migrationBuilder.DropTable(
                name: "exercises");

            migrationBuilder.DropTable(
                name: "materials");

            migrationBuilder.DropTable(
                name: "topics");
        }
    }
}
