using Microsoft.EntityFrameworkCore.Migrations;

namespace CizimApp.Migrations
{
    public partial class ConnectedRoomName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "roomAdmin",
                table: "Rooms",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConnectedRoomName",
                table: "ConnectedUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "roomAdmin",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "ConnectedRoomName",
                table: "ConnectedUsers");
        }
    }
}
