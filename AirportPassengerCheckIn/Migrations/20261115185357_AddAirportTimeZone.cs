using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirportPassengerCheckIn.Migrations
{
    /// <inheritdoc />
    public partial class AddAirportTimeZone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "Airports",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "Airports");
        }
    }
}
