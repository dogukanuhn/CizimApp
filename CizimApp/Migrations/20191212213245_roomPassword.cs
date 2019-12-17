using Microsoft.EntityFrameworkCore.Migrations;

namespace CizimApp.Migrations
{
    public partial class roomPassword : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "roomPassword",
                table: "Rooms",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "roomPassword",
                table: "Rooms");
        }
    }
}
