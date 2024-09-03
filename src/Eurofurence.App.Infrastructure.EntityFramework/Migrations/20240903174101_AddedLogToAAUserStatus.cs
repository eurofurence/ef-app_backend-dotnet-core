using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eurofurence.App.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddedLogToAAUserStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ArtistAlleyUserStatusRecords",
                table: "ArtistAlleyUserStatusRecords");

            migrationBuilder.RenameTable(
                name: "ArtistAlleyUserStatusRecords",
                newName: "ArtistAlleyUserStatus");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArtistAlleyUserStatus",
                table: "ArtistAlleyUserStatus",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ArtistAlleyUserStatusChanged",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ChangedDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ChangedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OldStatus = table.Column<int>(type: "int", nullable: false),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserStatusRecordID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LastChangeDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistAlleyUserStatusChanged", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtistAlleyUserStatusChanged_ArtistAlleyUserStatus_UserStatu~",
                        column: x => x.UserStatusRecordID,
                        principalTable: "ArtistAlleyUserStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistAlleyUserStatusChanged_UserStatusRecordID",
                table: "ArtistAlleyUserStatusChanged",
                column: "UserStatusRecordID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtistAlleyUserStatusChanged");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ArtistAlleyUserStatus",
                table: "ArtistAlleyUserStatus");

            migrationBuilder.RenameTable(
                name: "ArtistAlleyUserStatus",
                newName: "ArtistAlleyUserStatusRecords");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArtistAlleyUserStatusRecords",
                table: "ArtistAlleyUserStatusRecords",
                column: "Id");
        }
    }
}
