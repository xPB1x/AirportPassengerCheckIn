using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirportPassengerCheckIn.Migrations
{
    /// <inheritdoc />
    public partial class BookingStatusAndSeatClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Class",
                table: "Seats",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Bookings",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Class",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Bookings");
        }
    }
}
