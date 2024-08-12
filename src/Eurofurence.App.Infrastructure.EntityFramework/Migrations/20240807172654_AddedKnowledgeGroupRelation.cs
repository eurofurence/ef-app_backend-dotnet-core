using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eurofurence.App.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddedKnowledgeGroupRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_KnowledgeEntries_KnowledgeGroupId",
                table: "KnowledgeEntries",
                column: "KnowledgeGroupId");

            migrationBuilder.AddForeignKey(
                name: "FK_KnowledgeEntries_KnowledgeGroups_KnowledgeGroupId",
                table: "KnowledgeEntries",
                column: "KnowledgeGroupId",
                principalTable: "KnowledgeGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KnowledgeEntries_KnowledgeGroups_KnowledgeGroupId",
                table: "KnowledgeEntries");

            migrationBuilder.DropIndex(
                name: "IX_KnowledgeEntries_KnowledgeGroupId",
                table: "KnowledgeEntries");
        }
    }
}
