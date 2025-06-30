using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stockat.EF.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuctionRel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockId",
                table: "Auction",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Auction_StockId",
                table: "Auction",
                column: "StockId");

            migrationBuilder.AddForeignKey(
                name: "FK_Auction_Stocks_StockId",
                table: "Auction",
                column: "StockId",
                principalTable: "Stocks",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auction_Stocks_StockId",
                table: "Auction");

            migrationBuilder.DropIndex(
                name: "IX_Auction_StockId",
                table: "Auction");

            migrationBuilder.DropColumn(
                name: "StockId",
                table: "Auction");
        }
    }
}
