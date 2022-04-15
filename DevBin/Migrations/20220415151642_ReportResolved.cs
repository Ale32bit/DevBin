using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevBin.Migrations
{
    public partial class ReportResolved : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsClosed",
                table: "Reports",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsClosed",
                table: "Reports");
        }
    }
}
