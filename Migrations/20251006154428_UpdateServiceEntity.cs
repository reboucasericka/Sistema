using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sistema.Migrations
{
    /// <inheritdoc />
    public partial class UpdateServiceEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Services");

            migrationBuilder.AddColumn<int>(
                name: "DurationInMinutes",
                table: "Services",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationInMinutes",
                table: "Services");

            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "Services",
                type: "int",
                nullable: true);
        }
    }
}
