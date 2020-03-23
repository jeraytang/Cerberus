using Microsoft.EntityFrameworkCore.Migrations;

namespace Cerberus.API.Migrations
{
    public partial class RemoveUselessFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RolePermission_Permission_PermissionId",
                table: "RolePermission");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermission_Role_RoleId",
                table: "RolePermission");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermission_Role_RoleId1",
                table: "RolePermission");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermission_Permission_PermissionId",
                table: "UserPermission");

            migrationBuilder.DropForeignKey(
                name: "FK_UserPermission_User_UserId",
                table: "UserPermission");

            migrationBuilder.DropIndex(
                name: "IX_UserPermission_UserId",
                table: "UserPermission");

            migrationBuilder.DropIndex(
                name: "IX_RolePermission_RoleId",
                table: "RolePermission");

            migrationBuilder.DropIndex(
                name: "IX_RolePermission_RoleId1",
                table: "RolePermission");

            migrationBuilder.DropColumn(
                name: "RoleId1",
                table: "RolePermission");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserPermission",
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(40) CHARACTER SET utf8mb4",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "PermissionId",
                table: "UserPermission",
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(40) CHARACTER SET utf8mb4",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "RolePermission",
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(40) CHARACTER SET utf8mb4",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "PermissionId",
                table: "RolePermission",
                maxLength: 36,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(40) CHARACTER SET utf8mb4",
                oldMaxLength: 40);

            migrationBuilder.CreateIndex(
                name: "IX_User_CreationTime",
                table: "User",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_User_Email",
                table: "User",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_User_LastModificationTime",
                table: "User",
                column: "LastModificationTime");

            migrationBuilder.CreateIndex(
                name: "IX_User_Source",
                table: "User",
                column: "Source");

            migrationBuilder.CreateIndex(
                name: "IX_Service_CreationTime",
                table: "Service",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_Service_LastModificationTime",
                table: "Service",
                column: "LastModificationTime");

            migrationBuilder.CreateIndex(
                name: "IX_Role_CreationTime",
                table: "Role",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_Role_LastModificationTime",
                table: "Role",
                column: "LastModificationTime");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_CreationTime",
                table: "Permission",
                column: "CreationTime");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_LastModificationTime",
                table: "Permission",
                column: "LastModificationTime");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_CreationTime",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_Email",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_LastModificationTime",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_User_Source",
                table: "User");

            migrationBuilder.DropIndex(
                name: "IX_Service_CreationTime",
                table: "Service");

            migrationBuilder.DropIndex(
                name: "IX_Service_LastModificationTime",
                table: "Service");

            migrationBuilder.DropIndex(
                name: "IX_Role_CreationTime",
                table: "Role");

            migrationBuilder.DropIndex(
                name: "IX_Role_LastModificationTime",
                table: "Role");

            migrationBuilder.DropIndex(
                name: "IX_Permission_CreationTime",
                table: "Permission");

            migrationBuilder.DropIndex(
                name: "IX_Permission_LastModificationTime",
                table: "Permission");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserPermission",
                type: "varchar(40) CHARACTER SET utf8mb4",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 36);

            migrationBuilder.AlterColumn<string>(
                name: "PermissionId",
                table: "UserPermission",
                type: "varchar(40) CHARACTER SET utf8mb4",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 36);

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "RolePermission",
                type: "varchar(40) CHARACTER SET utf8mb4",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 36);

            migrationBuilder.AlterColumn<string>(
                name: "PermissionId",
                table: "RolePermission",
                type: "varchar(40) CHARACTER SET utf8mb4",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 36);

            migrationBuilder.AddColumn<string>(
                name: "RoleId1",
                table: "RolePermission",
                type: "varchar(255) CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPermission_UserId",
                table: "UserPermission",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_RoleId",
                table: "RolePermission",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_RoleId1",
                table: "RolePermission",
                column: "RoleId1");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermission_Permission_PermissionId",
                table: "RolePermission",
                column: "PermissionId",
                principalTable: "Permission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermission_Role_RoleId",
                table: "RolePermission",
                column: "RoleId",
                principalTable: "Role",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermission_Role_RoleId1",
                table: "RolePermission",
                column: "RoleId1",
                principalTable: "Role",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermission_Permission_PermissionId",
                table: "UserPermission",
                column: "PermissionId",
                principalTable: "Permission",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserPermission_User_UserId",
                table: "UserPermission",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
