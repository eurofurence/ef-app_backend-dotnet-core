using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eurofurence.App.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddedEventFeedbackRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_EventConferenceRoomRecord_ConferenceRoomId",
                table: "Events");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventConferenceRoomRecord",
                table: "EventConferenceRoomRecord");

            migrationBuilder.RenameTable(
                name: "EventConferenceRoomRecord",
                newName: "EventConferenceRooms");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventConferenceRooms",
                table: "EventConferenceRooms",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_EventFeedbacks_EventId",
                table: "EventFeedbacks",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventFeedbacks_Events_EventId",
                table: "EventFeedbacks",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_EventConferenceRooms_ConferenceRoomId",
                table: "Events",
                column: "ConferenceRoomId",
                principalTable: "EventConferenceRooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventFeedbacks_Events_EventId",
                table: "EventFeedbacks");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_EventConferenceRooms_ConferenceRoomId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_EventFeedbacks_EventId",
                table: "EventFeedbacks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventConferenceRooms",
                table: "EventConferenceRooms");

            migrationBuilder.RenameTable(
                name: "EventConferenceRooms",
                newName: "EventConferenceRoomRecord");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventConferenceRoomRecord",
                table: "EventConferenceRoomRecord",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_EventConferenceRoomRecord_ConferenceRoomId",
                table: "Events",
                column: "ConferenceRoomId",
                principalTable: "EventConferenceRoomRecord",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
