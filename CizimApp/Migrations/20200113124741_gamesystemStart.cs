using Microsoft.EntityFrameworkCore.Migrations;

namespace CizimApp.Migrations
{
    public partial class gamesystemStart : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "roomIsGameStart",
                table: "Rooms",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "roomPoint",
                table: "Rooms",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GamePoint",
                table: "ConnectedUsers",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "roomIsGameStart",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "roomPoint",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "GamePoint",
                table: "ConnectedUsers");
        }
    }
}
