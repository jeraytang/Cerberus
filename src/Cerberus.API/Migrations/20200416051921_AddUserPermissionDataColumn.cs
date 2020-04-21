using Microsoft.EntityFrameworkCore.Migrations;

namespace Cerberus.API.Migrations
{
    public partial class AddUserPermissionDataColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Data",
                table: "UserPermission",
                maxLength: 2000,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "UserPermission");
        }
    }
}
