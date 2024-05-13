using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eurofurence.App.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddedImageRelationToImageContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                table: "ImageContents",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");

            migrationBuilder.CreateIndex(
                name: "IX_ImageContents_ImageId",
                table: "ImageContents",
                column: "ImageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ImageContents_Images_ImageId",
                table: "ImageContents",
                column: "ImageId",
                principalTable: "Images",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImageContents_Images_ImageId",
                table: "ImageContents");

            migrationBuilder.DropIndex(
                name: "IX_ImageContents_ImageId",
                table: "ImageContents");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "ImageContents");
        }
    }
}
