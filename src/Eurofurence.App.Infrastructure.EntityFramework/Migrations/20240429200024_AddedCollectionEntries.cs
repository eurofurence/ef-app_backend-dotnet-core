using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eurofurence.App.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddedCollectionEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LinkFragments_Dealers_DealerRecordId",
                table: "LinkFragments");

            migrationBuilder.DropForeignKey(
                name: "FK_LinkFragments_KnowledgeEntries_KnowledgeEntryRecordId",
                table: "LinkFragments");

            migrationBuilder.DropForeignKey(
                name: "FK_LinkFragments_MapEntries_MapEntryRecordId",
                table: "LinkFragments");

            migrationBuilder.DropTable(
                name: "CollectionEntry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LinkFragments",
                table: "LinkFragments");

            migrationBuilder.RenameTable(
                name: "LinkFragments",
                newName: "LinkFragment");

            migrationBuilder.RenameIndex(
                name: "IX_LinkFragments_MapEntryRecordId",
                table: "LinkFragment",
                newName: "IX_LinkFragment_MapEntryRecordId");

            migrationBuilder.RenameIndex(
                name: "IX_LinkFragments_KnowledgeEntryRecordId",
                table: "LinkFragment",
                newName: "IX_LinkFragment_KnowledgeEntryRecordId");

            migrationBuilder.RenameIndex(
                name: "IX_LinkFragments_DealerRecordId",
                table: "LinkFragment",
                newName: "IX_LinkFragment_DealerRecordId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LinkFragment",
                table: "LinkFragment",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CollectionEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    PlayerParticipationId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FursuitParticipationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EventDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FursuitParticipationRecordId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    PlayerParticipationRecordId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    LastChangeDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionEntries_FursuitParticipations_FursuitParticipation~",
                        column: x => x.FursuitParticipationRecordId,
                        principalTable: "FursuitParticipations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CollectionEntries_PlayerParticipations_PlayerParticipationRe~",
                        column: x => x.PlayerParticipationRecordId,
                        principalTable: "PlayerParticipations",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionEntries_FursuitParticipationRecordId",
                table: "CollectionEntries",
                column: "FursuitParticipationRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionEntries_PlayerParticipationRecordId",
                table: "CollectionEntries",
                column: "PlayerParticipationRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_LinkFragment_Dealers_DealerRecordId",
                table: "LinkFragment",
                column: "DealerRecordId",
                principalTable: "Dealers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LinkFragment_KnowledgeEntries_KnowledgeEntryRecordId",
                table: "LinkFragment",
                column: "KnowledgeEntryRecordId",
                principalTable: "KnowledgeEntries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LinkFragment_MapEntries_MapEntryRecordId",
                table: "LinkFragment",
                column: "MapEntryRecordId",
                principalTable: "MapEntries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LinkFragment_Dealers_DealerRecordId",
                table: "LinkFragment");

            migrationBuilder.DropForeignKey(
                name: "FK_LinkFragment_KnowledgeEntries_KnowledgeEntryRecordId",
                table: "LinkFragment");

            migrationBuilder.DropForeignKey(
                name: "FK_LinkFragment_MapEntries_MapEntryRecordId",
                table: "LinkFragment");

            migrationBuilder.DropTable(
                name: "CollectionEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LinkFragment",
                table: "LinkFragment");

            migrationBuilder.RenameTable(
                name: "LinkFragment",
                newName: "LinkFragments");

            migrationBuilder.RenameIndex(
                name: "IX_LinkFragment_MapEntryRecordId",
                table: "LinkFragments",
                newName: "IX_LinkFragments_MapEntryRecordId");

            migrationBuilder.RenameIndex(
                name: "IX_LinkFragment_KnowledgeEntryRecordId",
                table: "LinkFragments",
                newName: "IX_LinkFragments_KnowledgeEntryRecordId");

            migrationBuilder.RenameIndex(
                name: "IX_LinkFragment_DealerRecordId",
                table: "LinkFragments",
                newName: "IX_LinkFragments_DealerRecordId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LinkFragments",
                table: "LinkFragments",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "CollectionEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EventDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FursuitParticipationId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    FursuitParticipationRecordId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    LastChangeDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    PlayerParticipationId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PlayerParticipationRecordId = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CollectionEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CollectionEntry_FursuitParticipations_FursuitParticipationRe~",
                        column: x => x.FursuitParticipationRecordId,
                        principalTable: "FursuitParticipations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CollectionEntry_PlayerParticipations_PlayerParticipationReco~",
                        column: x => x.PlayerParticipationRecordId,
                        principalTable: "PlayerParticipations",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionEntry_FursuitParticipationRecordId",
                table: "CollectionEntry",
                column: "FursuitParticipationRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_CollectionEntry_PlayerParticipationRecordId",
                table: "CollectionEntry",
                column: "PlayerParticipationRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_LinkFragments_Dealers_DealerRecordId",
                table: "LinkFragments",
                column: "DealerRecordId",
                principalTable: "Dealers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LinkFragments_KnowledgeEntries_KnowledgeEntryRecordId",
                table: "LinkFragments",
                column: "KnowledgeEntryRecordId",
                principalTable: "KnowledgeEntries",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LinkFragments_MapEntries_MapEntryRecordId",
                table: "LinkFragments",
                column: "MapEntryRecordId",
                principalTable: "MapEntries",
                principalColumn: "Id");
        }
    }
}
