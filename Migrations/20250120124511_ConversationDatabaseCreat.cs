using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_Chatbot.Migrations
{
    /// <inheritdoc />
    public partial class ConversationDatabaseCreat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Users_UserId1",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_UserId1",
                table: "Conversations");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Conversations");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Conversations",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_UserId",
                table: "Conversations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Users_UserId",
                table: "Conversations",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_Users_UserId",
                table: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_Conversations_UserId",
                table: "Conversations");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Conversations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "Conversations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_UserId1",
                table: "Conversations",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_Users_UserId1",
                table: "Conversations",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
