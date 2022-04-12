using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevBin.Migrations
{
    public partial class SyntaxKeyDisplayNameToName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pastes_Syntaxes_SyntaxDisplayName",
                table: "Pastes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Syntaxes",
                table: "Syntaxes");

            migrationBuilder.DropIndex(
                name: "IX_Pastes_SyntaxDisplayName",
                table: "Pastes");

            migrationBuilder.DropColumn(
                name: "SyntaxDisplayName",
                table: "Pastes");

            migrationBuilder.AlterColumn<string>(
                name: "SyntaxName",
                table: "Pastes",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Syntaxes",
                table: "Syntaxes",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Pastes_SyntaxName",
                table: "Pastes",
                column: "SyntaxName");

            migrationBuilder.AddForeignKey(
                name: "FK_Pastes_Syntaxes_SyntaxName",
                table: "Pastes",
                column: "SyntaxName",
                principalTable: "Syntaxes",
                principalColumn: "Name",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pastes_Syntaxes_SyntaxName",
                table: "Pastes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Syntaxes",
                table: "Syntaxes");

            migrationBuilder.DropIndex(
                name: "IX_Pastes_SyntaxName",
                table: "Pastes");

            migrationBuilder.AlterColumn<string>(
                name: "SyntaxName",
                table: "Pastes",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SyntaxDisplayName",
                table: "Pastes",
                type: "varchar(255)",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Syntaxes",
                table: "Syntaxes",
                column: "DisplayName");

            migrationBuilder.CreateIndex(
                name: "IX_Pastes_SyntaxDisplayName",
                table: "Pastes",
                column: "SyntaxDisplayName");

            migrationBuilder.AddForeignKey(
                name: "FK_Pastes_Syntaxes_SyntaxDisplayName",
                table: "Pastes",
                column: "SyntaxDisplayName",
                principalTable: "Syntaxes",
                principalColumn: "DisplayName",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
