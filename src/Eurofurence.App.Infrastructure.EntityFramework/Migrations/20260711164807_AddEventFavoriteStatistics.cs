using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eurofurence.App.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddEventFavoriteStatistics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventFavoriteStatistics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EventId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    UserRegistrationStatus = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<int>(type: "int", nullable: false),
                    LastChangeDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventFavoriteStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventFavoriteStatistics_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_EventFavoriteStatistics_EventId",
                table: "EventFavoriteStatistics",
                column: "EventId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventFavoriteStatistics");
        }
    }
}
