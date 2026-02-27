using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E7gezhaa.API.Migrations
{
    /// <inheritdoc />
    public partial class AddPhotographerSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "VenueId",
                table: "Bookings",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "PhotographerPackageId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PhotographerPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DurationInHours = table.Column<int>(type: "int", nullable: false),
                    Available = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhotographerPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhotographerPackages_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_PhotographerPackageId",
                table: "Bookings",
                column: "PhotographerPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_PhotographerPackages_VendorId",
                table: "PhotographerPackages",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_PhotographerPackages_PhotographerPackageId",
                table: "Bookings",
                column: "PhotographerPackageId",
                principalTable: "PhotographerPackages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_PhotographerPackages_PhotographerPackageId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "PhotographerPackages");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_PhotographerPackageId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "PhotographerPackageId",
                table: "Bookings");

            migrationBuilder.AlterColumn<int>(
                name: "VenueId",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
