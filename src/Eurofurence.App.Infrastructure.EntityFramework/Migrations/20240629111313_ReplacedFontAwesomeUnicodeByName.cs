using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eurofurence.App.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class ReplacedFontAwesomeUnicodeByName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FontAwesomeIconCharacterUnicodeAddress",
                table: "KnowledgeGroups",
                newName: "FontAwesomeIconName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FontAwesomeIconName",
                table: "KnowledgeGroups",
                newName: "FontAwesomeIconCharacterUnicodeAddress");
        }
    }
}
