using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E7gezhaa.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookingId1",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingId1",
                table: "Payments",
                column: "BookingId1",
                unique: true,
                filter: "[BookingId1] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Bookings_BookingId1",
                table: "Payments",
                column: "BookingId1",
                principalTable: "Bookings",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payments_Bookings_BookingId1",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_Payments_BookingId1",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "BookingId1",
                table: "Payments");
        }
    }
}
