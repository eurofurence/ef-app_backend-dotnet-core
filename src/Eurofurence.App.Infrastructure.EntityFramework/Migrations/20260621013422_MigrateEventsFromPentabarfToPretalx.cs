using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eurofurence.App.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class MigrateEventsFromPentabarfToPretalx : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Events");

            migrationBuilder.RenameColumn(
                name: "SourceEventId",
                table: "Events",
                newName: "SourceId");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "EventConferenceTracks",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "EventConferenceTracks",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "SourceId",
                table: "EventConferenceTracks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "EventConferenceRooms",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "EventConferenceRooms",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "SourceId",
                table: "EventConferenceRooms",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Color",
                table: "EventConferenceTracks");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "EventConferenceTracks");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "EventConferenceTracks");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "EventConferenceRooms");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "EventConferenceRooms");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "EventConferenceRooms");

            migrationBuilder.RenameColumn(
                name: "SourceId",
                table: "Events",
                newName: "SourceEventId");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "Events",
                type: "time(6)",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "Events",
                type: "time(6)",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
