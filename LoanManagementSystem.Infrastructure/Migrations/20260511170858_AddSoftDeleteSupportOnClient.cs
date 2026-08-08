using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoanManagementSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteSupportOnClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                schema: "dbo",
                table: "Clients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeletedById",
                schema: "dbo",
                table: "Clients",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "dbo",
                table: "Clients",
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
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                schema: "dbo",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "dbo",
                table: "Clients");
        }
    }
}
