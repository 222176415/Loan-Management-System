using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteSupportOnUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "dbo",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedById",
                schema: "dbo",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "dbo",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "dbo",
                table: "Users");
        }
    }
}
