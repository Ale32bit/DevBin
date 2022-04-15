using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevBin.Migrations
{
    public partial class ReportFix2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReporterIPAddress",
                table: "Reports",
                type: "varchar(42)",
                maxLength: 42,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(45)")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReporterIPAddress",
                table: "Reports",
                type: "varchar(45)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(42)",
                oldMaxLength: 42)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
