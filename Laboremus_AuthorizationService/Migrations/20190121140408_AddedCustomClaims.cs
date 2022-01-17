using Microsoft.EntityFrameworkCore.Migrations;

namespace Laboremus_AuthorizationService.Migrations
{
    public partial class AddedCustomClaims : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomClaims",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    ClaimsData = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomClaims", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomClaims");
        }
    }
}
