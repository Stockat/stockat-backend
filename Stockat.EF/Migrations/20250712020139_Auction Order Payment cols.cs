using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stockat.EF.Migrations
{
    /// <inheritdoc />
    public partial class AuctionOrderPaymentcols : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PaymentStatus",
                table: "AuctionOrder",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripePaymentIntentId",
                table: "AuctionOrder",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeSessionId",
                table: "AuctionOrder",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StripePaymentIntentId",
                table: "AuctionOrder");

            migrationBuilder.DropColumn(
                name: "StripeSessionId",
                table: "AuctionOrder");

            migrationBuilder.AlterColumn<bool>(
                name: "PaymentStatus",
                table: "AuctionOrder",
                type: "bit",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
