using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eurofurence.App.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class ReplacedStringListsWithEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Roles",
                table: "RegSysIdentities");

            migrationBuilder.DropColumn(
                name: "GrantRoles",
                table: "RegSysAccessTokens");

            migrationBuilder.DropColumn(
                name: "Topics",
                table: "PushNotificationChannels");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegSysAccessTokenRecordId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    RegSysIdentityRecordId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LastChangeDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Roles_RegSysAccessTokens_RegSysAccessTokenRecordId",
                        column: x => x.RegSysAccessTokenRecordId,
                        principalTable: "RegSysAccessTokens",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Roles_RegSysIdentities_RegSysIdentityRecordId",
                        column: x => x.RegSysIdentityRecordId,
                        principalTable: "RegSysIdentities",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PushNotificationChannelRecordId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LastChangeDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Topics_PushNotificationChannels_PushNotificationChannelRecor~",
                        column: x => x.PushNotificationChannelRecordId,
                        principalTable: "PushNotificationChannels",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_RegSysAccessTokenRecordId",
                table: "Roles",
                column: "RegSysAccessTokenRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_RegSysIdentityRecordId",
                table: "Roles",
                column: "RegSysIdentityRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_PushNotificationChannelRecordId",
                table: "Topics",
                column: "PushNotificationChannelRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.AddColumn<string>(
                name: "Roles",
                table: "RegSysIdentities",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "GrantRoles",
                table: "RegSysAccessTokens",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Topics",
                table: "PushNotificationChannels",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
