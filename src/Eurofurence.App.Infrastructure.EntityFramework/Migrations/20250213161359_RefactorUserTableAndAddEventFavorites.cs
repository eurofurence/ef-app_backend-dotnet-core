using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eurofurence.App.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RefactorUserTableAndAddEventFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrationIdentities");

            migrationBuilder.DropColumn(
                name: "Acl",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Users",
                newName: "Nickname");

            migrationBuilder.AddColumn<string>(
                name: "IdentityId",
                table: "Users",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RegSysId",
                table: "Users",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<Guid>(
                name: "TelegramUserRecordId",
                table: "Events",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "EventRecordUserRecord",
                columns: table => new
                {
                    FavoredById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FavoriteEventsId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRecordUserRecord", x => new { x.FavoredById, x.FavoriteEventsId });
                    table.ForeignKey(
                        name: "FK_EventRecordUserRecord_Events_FavoriteEventsId",
                        column: x => x.FavoriteEventsId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventRecordUserRecord_Users_FavoredById",
                        column: x => x.FavoredById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TelegramUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Username = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Acl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegsysID = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastChangeDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TelegramUsers", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Events_TelegramUserRecordId",
                table: "Events",
                column: "TelegramUserRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_EventRecordUserRecord_FavoriteEventsId",
                table: "EventRecordUserRecord",
                column: "FavoriteEventsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_TelegramUsers_TelegramUserRecordId",
                table: "Events",
                column: "TelegramUserRecordId",
                principalTable: "TelegramUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_TelegramUsers_TelegramUserRecordId",
                table: "Events");

            migrationBuilder.DropTable(
                name: "EventRecordUserRecord");

            migrationBuilder.DropTable(
                name: "TelegramUsers");

            migrationBuilder.DropIndex(
                name: "IX_Events_TelegramUserRecordId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "IdentityId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RegSysId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TelegramUserRecordId",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "Nickname",
                table: "Users",
                newName: "Username");

            migrationBuilder.AddColumn<string>(
                name: "Acl",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RegistrationIdentities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IdentityId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    LastChangeDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RegSysId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationIdentities", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
