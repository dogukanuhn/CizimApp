using Microsoft.EntityFrameworkCore.Migrations;

namespace CizimApp.Migrations
{
    public partial class roomUserCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "roomUserCount",
                table: "Rooms",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "roomUserCount",
                table: "Rooms");
        }
    }
}
