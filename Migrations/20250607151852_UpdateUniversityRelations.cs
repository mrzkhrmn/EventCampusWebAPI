using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventCampusAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUniversityRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                table: "Universities",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShortName",
                table: "Universities");
        }
    }
}
