using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eurofurence.App.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCalendarToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_TelegramUsers_TelegramUserRecordId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_TelegramUserRecordId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RegsysID",
                table: "TelegramUsers");

            migrationBuilder.DropColumn(
                name: "TelegramUserRecordId",
                table: "Events");

            migrationBuilder.AddColumn<string>(
                name: "CalendarToken",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalendarToken",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "RegsysID",
                table: "TelegramUsers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "TelegramUserRecordId",
                table: "Events",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Events_TelegramUserRecordId",
                table: "Events",
                column: "TelegramUserRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_TelegramUsers_TelegramUserRecordId",
                table: "Events",
                column: "TelegramUserRecordId",
                principalTable: "TelegramUsers",
                principalColumn: "Id");
        }
    }
}
