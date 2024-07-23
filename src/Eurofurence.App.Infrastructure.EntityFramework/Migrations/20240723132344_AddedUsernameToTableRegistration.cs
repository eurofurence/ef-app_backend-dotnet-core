using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eurofurence.App.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddedUsernameToTableRegistration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MapEntries_Maps_MapId",
                table: "MapEntries");

            migrationBuilder.AddColumn<string>(
                name: "OwnerUsername",
                table: "TableRegistrations",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<Guid>(
                name: "MapId",
                table: "MapEntries",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_MapEntries_Maps_MapId",
                table: "MapEntries",
                column: "MapId",
                principalTable: "Maps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MapEntries_Maps_MapId",
                table: "MapEntries");

            migrationBuilder.DropColumn(
                name: "OwnerUsername",
                table: "TableRegistrations");

            migrationBuilder.AlterColumn<Guid>(
                name: "MapId",
                table: "MapEntries",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AddForeignKey(
                name: "FK_MapEntries_Maps_MapId",
                table: "MapEntries",
                column: "MapId",
                principalTable: "Maps",
                principalColumn: "Id");
        }
    }
}
