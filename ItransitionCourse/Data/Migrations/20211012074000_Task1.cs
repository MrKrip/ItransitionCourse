using Microsoft.EntityFrameworkCore.Migrations;

namespace ItransitionCourse.Data.Migrations
{
    public partial class Task1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Theme",
                table: "Tasks",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Theme",
                table: "Tasks");
        }
    }
}
