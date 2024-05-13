using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eurofurence.App.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class ReplacedImageIdsByImageRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageIds",
                table: "KnowledgeEntries");

            migrationBuilder.AddColumn<Guid>(
                name: "KnowledgeEntryRecordId",
                table: "Images",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_Images_KnowledgeEntryRecordId",
                table: "Images",
                column: "KnowledgeEntryRecordId");

            migrationBuilder.AddForeignKey(
                name: "FK_Images_KnowledgeEntries_KnowledgeEntryRecordId",
                table: "Images",
                column: "KnowledgeEntryRecordId",
                principalTable: "KnowledgeEntries",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_KnowledgeEntries_KnowledgeEntryRecordId",
                table: "Images");

            migrationBuilder.DropIndex(
                name: "IX_Images_KnowledgeEntryRecordId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "KnowledgeEntryRecordId",
                table: "Images");

            migrationBuilder.AddColumn<string>(
                name: "ImageIds",
                table: "KnowledgeEntries",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
