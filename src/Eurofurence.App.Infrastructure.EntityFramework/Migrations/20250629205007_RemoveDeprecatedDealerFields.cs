using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eurofurence.App.Infrastructure.EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDeprecatedDealerFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendeeNickname",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "Dealers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttendeeNickname",
                table: "Dealers",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "RegistrationNumber",
                table: "Dealers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
