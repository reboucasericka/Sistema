using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sistema.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePhotoPathFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Professionals");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Photo",
                table: "Services",
                newName: "PhotoPath");

            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "Professionals",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "Products",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "Customers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "Professionals");

            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "Customers");

            migrationBuilder.RenameColumn(
                name: "PhotoPath",
                table: "Services",
                newName: "Photo");

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Professionals",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ImageId",
                table: "Products",
                type: "uniqueidentifier",
                maxLength: 200,
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }
    }
}
