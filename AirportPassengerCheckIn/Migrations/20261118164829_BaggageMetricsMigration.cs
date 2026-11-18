using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirportPassengerCheckIn.Migrations
{
    /// <inheritdoc />
    public partial class BaggageMetricsMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MaxBaggageWeight",
                table: "Flights",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "CountPaidBaggages",
                table: "BookingPassengers",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxBaggageWeight",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "CountPaidBaggages",
                table: "BookingPassengers");
        }
    }
}
