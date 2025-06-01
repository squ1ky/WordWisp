using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WordWisp.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPublicField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dictionaries_users_user_id",
                table: "Dictionaries");

            migrationBuilder.DropForeignKey(
                name: "FK_Words_Dictionaries_dictionary_id",
                table: "Words");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Words",
                table: "Words");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Dictionaries",
                table: "Dictionaries");

            migrationBuilder.RenameTable(
                name: "Words",
                newName: "words");

            migrationBuilder.RenameTable(
                name: "Dictionaries",
                newName: "dictionaries");

            migrationBuilder.RenameIndex(
                name: "IX_Words_dictionary_id",
                table: "words",
                newName: "IX_words_dictionary_id");

            migrationBuilder.RenameIndex(
                name: "IX_Dictionaries_user_id",
                table: "dictionaries",
                newName: "IX_dictionaries_user_id");

            migrationBuilder.AlterColumn<bool>(
                name: "is_public",
                table: "dictionaries",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddPrimaryKey(
                name: "PK_words",
                table: "words",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_dictionaries",
                table: "dictionaries",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_dictionaries_users_user_id",
                table: "dictionaries",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_words_dictionaries_dictionary_id",
                table: "words",
                column: "dictionary_id",
                principalTable: "dictionaries",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_dictionaries_users_user_id",
                table: "dictionaries");

            migrationBuilder.DropForeignKey(
                name: "FK_words_dictionaries_dictionary_id",
                table: "words");

            migrationBuilder.DropPrimaryKey(
                name: "PK_words",
                table: "words");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dictionaries",
                table: "dictionaries");

            migrationBuilder.RenameTable(
                name: "words",
                newName: "Words");

            migrationBuilder.RenameTable(
                name: "dictionaries",
                newName: "Dictionaries");

            migrationBuilder.RenameIndex(
                name: "IX_words_dictionary_id",
                table: "Words",
                newName: "IX_Words_dictionary_id");

            migrationBuilder.RenameIndex(
                name: "IX_dictionaries_user_id",
                table: "Dictionaries",
                newName: "IX_Dictionaries_user_id");

            migrationBuilder.AlterColumn<bool>(
                name: "is_public",
                table: "Dictionaries",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Words",
                table: "Words",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Dictionaries",
                table: "Dictionaries",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Dictionaries_users_user_id",
                table: "Dictionaries",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Words_Dictionaries_dictionary_id",
                table: "Words",
                column: "dictionary_id",
                principalTable: "Dictionaries",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
