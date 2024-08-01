using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eurofurence.App.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class ReworkedImageRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Images_KnowledgeEntries_KnowledgeEntryRecordId",
                table: "Images");

            migrationBuilder.DropForeignKey(
                name: "FK_MapEntries_Maps_MapId",
                table: "MapEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_Maps_Images_ImageId",
                table: "Maps");

            migrationBuilder.DropIndex(
                name: "IX_Images_KnowledgeEntryRecordId",
                table: "Images");

            migrationBuilder.DropColumn(
                name: "KnowledgeEntryRecordId",
                table: "Images");

            migrationBuilder.AlterColumn<Guid>(
                name: "ImageId",
                table: "Maps",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "MapId",
                table: "MapEntries",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.CreateTable(
                name: "ImageRecordKnowledgeEntryRecord",
                columns: table => new
                {
                    ImagesId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    KnowledgeEntriesId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageRecordKnowledgeEntryRecord", x => new { x.ImagesId, x.KnowledgeEntriesId });
                    table.ForeignKey(
                        name: "FK_ImageRecordKnowledgeEntryRecord_Images_ImagesId",
                        column: x => x.ImagesId,
                        principalTable: "Images",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImageRecordKnowledgeEntryRecord_KnowledgeEntries_KnowledgeEn~",
                        column: x => x.KnowledgeEntriesId,
                        principalTable: "KnowledgeEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Events_BannerImageId",
                table: "Events",
                column: "BannerImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_PosterImageId",
                table: "Events",
                column: "PosterImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_ArtistImageId",
                table: "Dealers",
                column: "ArtistImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_ArtistThumbnailImageId",
                table: "Dealers",
                column: "ArtistThumbnailImageId");

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_ArtPreviewImageId",
                table: "Dealers",
                column: "ArtPreviewImageId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageRecordKnowledgeEntryRecord_KnowledgeEntriesId",
                table: "ImageRecordKnowledgeEntryRecord",
                column: "KnowledgeEntriesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Dealers_Images_ArtPreviewImageId",
                table: "Dealers",
                column: "ArtPreviewImageId",
                principalTable: "Images",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Dealers_Images_ArtistImageId",
                table: "Dealers",
                column: "ArtistImageId",
                principalTable: "Images",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Dealers_Images_ArtistThumbnailImageId",
                table: "Dealers",
                column: "ArtistThumbnailImageId",
                principalTable: "Images",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Images_BannerImageId",
                table: "Events",
                column: "BannerImageId",
                principalTable: "Images",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Images_PosterImageId",
                table: "Events",
                column: "PosterImageId",
                principalTable: "Images",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MapEntries_Maps_MapId",
                table: "MapEntries",
                column: "MapId",
                principalTable: "Maps",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Maps_Images_ImageId",
                table: "Maps",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dealers_Images_ArtPreviewImageId",
                table: "Dealers");

            migrationBuilder.DropForeignKey(
                name: "FK_Dealers_Images_ArtistImageId",
                table: "Dealers");

            migrationBuilder.DropForeignKey(
                name: "FK_Dealers_Images_ArtistThumbnailImageId",
                table: "Dealers");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Images_BannerImageId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Images_PosterImageId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_MapEntries_Maps_MapId",
                table: "MapEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_Maps_Images_ImageId",
                table: "Maps");

            migrationBuilder.DropTable(
                name: "ImageRecordKnowledgeEntryRecord");

            migrationBuilder.DropIndex(
                name: "IX_Events_BannerImageId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_PosterImageId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Dealers_ArtistImageId",
                table: "Dealers");

            migrationBuilder.DropIndex(
                name: "IX_Dealers_ArtistThumbnailImageId",
                table: "Dealers");

            migrationBuilder.DropIndex(
                name: "IX_Dealers_ArtPreviewImageId",
                table: "Dealers");

            migrationBuilder.AlterColumn<Guid>(
                name: "ImageId",
                table: "Maps",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)",
                oldNullable: true)
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

            migrationBuilder.AlterColumn<Guid>(
                name: "MapId",
                table: "MapEntries",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci",
                oldClrType: typeof(Guid),
                oldType: "char(36)")
                .OldAnnotation("Relational:Collation", "ascii_general_ci");

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

            migrationBuilder.AddForeignKey(
                name: "FK_MapEntries_Maps_MapId",
                table: "MapEntries",
                column: "MapId",
                principalTable: "Maps",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Maps_Images_ImageId",
                table: "Maps",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
