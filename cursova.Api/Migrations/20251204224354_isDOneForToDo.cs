using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cursova.Api.Migrations
{
    /// <inheritdoc />
    public partial class isDOneForToDo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDone",
                table: "Trips",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDone",
                table: "Trips");
        }
    }
}
