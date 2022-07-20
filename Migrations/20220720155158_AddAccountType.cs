using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInfo.Migrations
{
    public partial class AddAccountType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConfirmPassword",
                table: "Users",
                newName: "Type");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Users",
                newName: "ConfirmPassword");
        }
    }
}
