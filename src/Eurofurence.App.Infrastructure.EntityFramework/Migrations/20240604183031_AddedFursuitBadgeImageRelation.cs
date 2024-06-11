using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eurofurence.App.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddedFursuitBadgeImageRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FursuitBadgeImages");

            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                table: "FursuitBadges",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_FursuitBadges_ImageId",
                table: "FursuitBadges",
                column: "ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_FursuitBadges_Images_ImageId",
                table: "FursuitBadges",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FursuitBadges_Images_ImageId",
                table: "FursuitBadges");

            migrationBuilder.DropIndex(
                name: "IX_FursuitBadges_ImageId",
                table: "FursuitBadges");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "FursuitBadges");

            migrationBuilder.CreateTable(
                name: "FursuitBadgeImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Height = table.Column<int>(type: "int", nullable: false),
                    ImageBytes = table.Column<byte[]>(type: "longblob", nullable: false),
                    IsDeleted = table.Column<int>(type: "int", nullable: false),
                    LastChangeDateTimeUtc = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MimeType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    SourceContentHashSha1 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Width = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FursuitBadgeImages", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
