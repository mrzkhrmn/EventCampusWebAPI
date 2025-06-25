using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventCampusAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddEventImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EventImagesJson",
                table: "Events",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventImagesJson",
                table: "Events");
        }
    }
}
