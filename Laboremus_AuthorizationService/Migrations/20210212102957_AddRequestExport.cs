using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Laboremus_AuthorizationService.Migrations
{
    public partial class AddRequestExport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExportRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    GenerationStatus = table.Column<int>(nullable: false),
                    CreatedOn = table.Column<DateTime>(nullable: false),
                    Request = table.Column<string>(nullable: true),
                    DownloadedOn = table.Column<DateTime>(nullable: true),
                    FileName = table.Column<string>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExportRequests", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExportRequests");
        }
    }
}
