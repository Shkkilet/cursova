using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cursova.Api.Migrations
{
    /// <inheritdoc />
    public partial class addedNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EndTripName",
                table: "Trips",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StartTripName",
                table: "Trips",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTripName",
                table: "Trips");

            migrationBuilder.DropColumn(
                name: "StartTripName",
                table: "Trips");
        }
    }
}
