using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E7gezhaa.API.Migrations
{
    /// <inheritdoc />
    public partial class SyncAllEntitiesToIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Vendors_VendorId1",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_VendorId1",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "VendorId1",
                table: "Reviews");

            migrationBuilder.AlterColumn<string>(
                name: "VendorId",
                table: "Reviews",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_VendorId",
                table: "Reviews",
                column: "VendorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Vendors_VendorId",
                table: "Reviews",
                column: "VendorId",
                principalTable: "Vendors",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Vendors_VendorId",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_VendorId",
                table: "Reviews");

            migrationBuilder.AlterColumn<int>(
                name: "VendorId",
                table: "Reviews",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VendorId1",
                table: "Reviews",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_VendorId1",
                table: "Reviews",
                column: "VendorId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Vendors_VendorId1",
                table: "Reviews",
                column: "VendorId1",
                principalTable: "Vendors",
                principalColumn: "Id");
        }
    }
}
