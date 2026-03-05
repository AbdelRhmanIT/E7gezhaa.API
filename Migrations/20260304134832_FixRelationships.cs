using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E7gezhaa.API.Migrations
{
    /// <inheritdoc />
    public partial class FixRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiSuggestions_Bookings_BookingId",
                table: "AiSuggestions");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeSlots_Venues_VenueId",
                table: "TimeSlots");

            migrationBuilder.DropForeignKey(
                name: "FK_VenueImages_Venues_VenueId",
                table: "VenueImages");

            migrationBuilder.AddForeignKey(
                name: "FK_AiSuggestions_Bookings_BookingId",
                table: "AiSuggestions",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeSlots_Venues_VenueId",
                table: "TimeSlots",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_VenueImages_Venues_VenueId",
                table: "VenueImages",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiSuggestions_Bookings_BookingId",
                table: "AiSuggestions");

            migrationBuilder.DropForeignKey(
                name: "FK_TimeSlots_Venues_VenueId",
                table: "TimeSlots");

            migrationBuilder.DropForeignKey(
                name: "FK_VenueImages_Venues_VenueId",
                table: "VenueImages");

            migrationBuilder.AddForeignKey(
                name: "FK_AiSuggestions_Bookings_BookingId",
                table: "AiSuggestions",
                column: "BookingId",
                principalTable: "Bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TimeSlots_Venues_VenueId",
                table: "TimeSlots",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VenueImages_Venues_VenueId",
                table: "VenueImages",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
