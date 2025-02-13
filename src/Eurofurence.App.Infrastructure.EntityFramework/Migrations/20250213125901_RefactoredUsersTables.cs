using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eurofurence.App.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RefactoredUsersTables : Migration
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

            migrationBuilder.CreateTable(
                name: "EventFavorites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EventId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserUid = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastChangeDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventFavorites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventFavorites_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
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

            migrationBuilder.CreateTable(
                name: "EventRecordTelegramUserRecord",
                columns: table => new
                {
                    FavoredById = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FavoriteEventsId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRecordTelegramUserRecord", x => new { x.FavoredById, x.FavoriteEventsId });
                    table.ForeignKey(
                        name: "FK_EventRecordTelegramUserRecord_Events_FavoriteEventsId",
                        column: x => x.FavoriteEventsId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventRecordTelegramUserRecord_TelegramUsers_FavoredById",
                        column: x => x.FavoredById,
                        principalTable: "TelegramUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EventFavorites_EventId",
                table: "EventFavorites",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventRecordTelegramUserRecord_FavoriteEventsId",
                table: "EventRecordTelegramUserRecord",
                column: "FavoriteEventsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventFavorites");

            migrationBuilder.DropTable(
                name: "EventRecordTelegramUserRecord");

            migrationBuilder.DropTable(
                name: "TelegramUsers");

            migrationBuilder.DropColumn(
                name: "IdentityId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RegSysId",
                table: "Users");

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
