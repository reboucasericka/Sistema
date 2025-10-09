using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sistema.Migrations
{
    /// <inheritdoc />
    public partial class Two : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Id",
                table: "Billings");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Service",
                newName: "ServiceId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PriceTables",
                newName: "PriceId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Notifications",
                newName: "NotificationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "Service",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PriceId",
                table: "PriceTables",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "NotificationId",
                table: "Notifications",
                newName: "Id");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Billings",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
