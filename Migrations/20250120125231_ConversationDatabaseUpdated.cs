using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AI_Chatbot.Migrations
{
    /// <inheritdoc />
    public partial class ConversationDatabaseUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Conversations");

            migrationBuilder.RenameColumn(
                name: "conversationIntent",
                table: "Conversations",
                newName: "Intent");

            migrationBuilder.AddColumn<string>(
                name: "Context",
                table: "Conversations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Context",
                table: "Conversations");

            migrationBuilder.RenameColumn(
                name: "Intent",
                table: "Conversations",
                newName: "conversationIntent");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Conversations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
