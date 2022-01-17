using Microsoft.EntityFrameworkCore.Migrations;

namespace Laboremus_AuthorizationService.Migrations
{
    public partial class RemovedTelephoneFromApplicationUserModel : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Telephone",
                table: "AspNetUsers");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Telephone",
                table: "AspNetUsers",
                nullable: true);
        }
    }
}
