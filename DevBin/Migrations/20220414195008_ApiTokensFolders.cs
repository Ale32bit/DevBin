using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevBin.Migrations
{
    public partial class ApiTokensFolders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowCreateFolders",
                table: "ApiTokens",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowDeleteFolders",
                table: "ApiTokens",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowCreateFolders",
                table: "ApiTokens");

            migrationBuilder.DropColumn(
                name: "AllowDeleteFolders",
                table: "ApiTokens");
        }
    }
}
