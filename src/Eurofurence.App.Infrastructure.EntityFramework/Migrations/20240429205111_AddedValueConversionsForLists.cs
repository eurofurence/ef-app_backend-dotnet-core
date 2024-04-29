using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eurofurence.App.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddedValueConversionsForLists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssueRecord");

            migrationBuilder.CreateTable(
                name: "IssueRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    RequesterUid = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NameOnBadge = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequestDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RegSysAlternativePinRecordId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LastChangeDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueRecords_RegSysAlternativePins_RegSysAlternativePinRecor~",
                        column: x => x.RegSysAlternativePinRecordId,
                        principalTable: "RegSysAlternativePins",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_IssueRecords_RegSysAlternativePinRecordId",
                table: "IssueRecords",
                column: "RegSysAlternativePinRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IssueRecords");

            migrationBuilder.CreateTable(
                name: "IssueRecord",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    LastChangeDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    NameOnBadge = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegSysAlternativePinRecordId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    RequestDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RequesterUid = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssueRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IssueRecord_RegSysAlternativePins_RegSysAlternativePinRecord~",
                        column: x => x.RegSysAlternativePinRecordId,
                        principalTable: "RegSysAlternativePins",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_IssueRecord_RegSysAlternativePinRecordId",
                table: "IssueRecord",
                column: "RegSysAlternativePinRecordId");
        }
    }
}
