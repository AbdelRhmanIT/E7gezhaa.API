using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E7gezhaa.API.Migrations
{
    /// <inheritdoc />
    public partial class AddBeautyPackagesAndBookingLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BeautyPackageId",
                table: "Bookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BeautyPackages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VendorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Available = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BeautyPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BeautyPackages_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_BeautyPackageId",
                table: "Bookings",
                column: "BeautyPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_BeautyPackages_VendorId",
                table: "BeautyPackages",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_BeautyPackages_BeautyPackageId",
                table: "Bookings",
                column: "BeautyPackageId",
                principalTable: "BeautyPackages",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_BeautyPackages_BeautyPackageId",
                table: "Bookings");

            migrationBuilder.DropTable(
                name: "BeautyPackages");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_BeautyPackageId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "BeautyPackageId",
                table: "Bookings");
        }
    }
}
