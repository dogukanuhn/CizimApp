using Microsoft.EntityFrameworkCore.Migrations;

namespace CizimApp.Migrations
{
    public partial class roomMaxUserCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "roomMaxUserCount",
                table: "Rooms",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "roomMaxUserCount",
                table: "Rooms");
        }
    }
}
