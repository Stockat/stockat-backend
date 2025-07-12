using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stockat.EF.Migrations
{
    /// <inheritdoc />
    public partial class Driverorderonetomany : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderProduct_DriverId",
                table: "OrderProduct");

            migrationBuilder.DropColumn(
                name: "AssignedOrderId",
                table: "Drivers");

            migrationBuilder.CreateIndex(
                name: "IX_OrderProduct_DriverId",
                table: "OrderProduct",
                column: "DriverId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderProduct_DriverId",
                table: "OrderProduct");

            migrationBuilder.AddColumn<int>(
                name: "AssignedOrderId",
                table: "Drivers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderProduct_DriverId",
                table: "OrderProduct",
                column: "DriverId",
                unique: true,
                filter: "[DriverId] IS NOT NULL");
        }
    }
}
