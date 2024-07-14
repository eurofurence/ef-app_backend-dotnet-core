using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eurofurence.App.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RenamedLinkFragments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
