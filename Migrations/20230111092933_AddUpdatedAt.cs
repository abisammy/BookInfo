using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookInfo.Migrations
{
    /// <inheritdoc />
    public partial class AddUpdatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Categories",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Books",
                newName: "UpdatedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Publishers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Authors",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Publishers");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Authors");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Categories",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Books",
                newName: "CreatedAt");
        }
    }
}
