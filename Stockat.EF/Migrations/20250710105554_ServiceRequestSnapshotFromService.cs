using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stockat.EF.Migrations
{
    /// <inheritdoc />
    public partial class ServiceRequestSnapshotFromService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeferred",
                table: "ServiceEditRequest");

            migrationBuilder.AddColumn<string>(
                name: "ServiceDescriptionSnapshot",
                table: "ServiceRequests",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceEstimatedTimeSnapshot",
                table: "ServiceRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceImageUrlSnapshot",
                table: "ServiceRequests",
                type: "nvarchar(2083)",
                maxLength: 2083,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceMinQuantitySnapshot",
                table: "ServiceRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ServiceNameSnapshot",
                table: "ServiceRequests",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ServicePricePerProductSnapshot",
                table: "ServiceRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceDescriptionSnapshot",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ServiceEstimatedTimeSnapshot",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ServiceImageUrlSnapshot",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ServiceMinQuantitySnapshot",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ServiceNameSnapshot",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ServicePricePerProductSnapshot",
                table: "ServiceRequests");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeferred",
                table: "ServiceEditRequest",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
