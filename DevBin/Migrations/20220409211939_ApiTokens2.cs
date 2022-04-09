using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevBin.Migrations
{
    public partial class ApiTokens2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiToken_AspNetUsers_OwnerId",
                table: "ApiToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApiToken",
                table: "ApiToken");

            migrationBuilder.RenameTable(
                name: "ApiToken",
                newName: "ApiTokens");

            migrationBuilder.RenameIndex(
                name: "IX_ApiToken_OwnerId",
                table: "ApiTokens",
                newName: "IX_ApiTokens_OwnerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApiTokens",
                table: "ApiTokens",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiTokens_AspNetUsers_OwnerId",
                table: "ApiTokens",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ApiTokens_AspNetUsers_OwnerId",
                table: "ApiTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ApiTokens",
                table: "ApiTokens");

            migrationBuilder.RenameTable(
                name: "ApiTokens",
                newName: "ApiToken");

            migrationBuilder.RenameIndex(
                name: "IX_ApiTokens_OwnerId",
                table: "ApiToken",
                newName: "IX_ApiToken_OwnerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ApiToken",
                table: "ApiToken",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ApiToken_AspNetUsers_OwnerId",
                table: "ApiToken",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
