using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eurofurence.App.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RenamedAATables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtistAlleyUserStatusChanged");

            migrationBuilder.DropTable(
                name: "ArtistAlleyUserStatus");

            migrationBuilder.CreateTable(
                name: "ArtistAlleyUserPenalties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Penalty = table.Column<int>(type: "int", nullable: false),
                    LastChangeDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistAlleyUserPenalties", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArtistAlleyUserPenaltiesChanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ChangedDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ChangedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OldPenalties = table.Column<int>(type: "int", nullable: false),
                    NewPenalties = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UserPenaltyRecordId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    LastChangeDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistAlleyUserPenaltiesChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ArtistAlleyUserPenaltiesChanges_ArtistAlleyUserPenalties_Use~",
                        column: x => x.UserPenaltyRecordId,
                        principalTable: "ArtistAlleyUserPenalties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistAlleyUserPenaltiesChanges_UserPenaltyRecordId",
                table: "ArtistAlleyUserPenaltiesChanges",
                column: "UserPenaltyRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArtistAlleyUserPenaltiesChanges");

            migrationBuilder.DropTable(
                name: "ArtistAlleyUserPenalties");

            migrationBuilder.CreateTable(
                name: "ArtistAlleyUserStatus",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    LastChangeDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistAlleyUserStatus", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArtistAlleyUserStatusChanged",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserStatusRecordID = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    ChangedBy = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ChangedDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    LastChangeDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    NewStatus = table.Column<int>(type: "int", nullable: false),
                    OldStatus = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
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
    }
}
