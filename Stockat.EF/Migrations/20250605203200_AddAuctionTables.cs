using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stockat.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddAuctionTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Auction",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StartingPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentBid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    IncrementUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    BuyerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BuyerUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    SellerId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Auction_AspNetUsers_BuyerUserId",
                        column: x => x.BuyerUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Auction_AspNetUsers_SellerId",
                        column: x => x.SellerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Auction_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AuctionBidRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuctionId = table.Column<int>(type: "int", nullable: false),
                    BidderId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BidderUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    BidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuctionBidRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuctionBidRequest_AspNetUsers_BidderUserId",
                        column: x => x.BidderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuctionBidRequest_Auction_AuctionId",
                        column: x => x.AuctionId,
                        principalTable: "Auction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuctionOrder",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: true),
                    PaymentTransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentStatus = table.Column<bool>(type: "bit", nullable: true),
                    AuctionId = table.Column<int>(type: "int", nullable: false),
                    AuctionRequestId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuctionOrder", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuctionOrder_AuctionBidRequest_AuctionRequestId",
                        column: x => x.AuctionRequestId,
                        principalTable: "AuctionBidRequest",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AuctionOrder_Auction_AuctionId",
                        column: x => x.AuctionId,
                        principalTable: "Auction",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Auction_BuyerUserId",
                table: "Auction",
                column: "BuyerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Auction_ProductId",
                table: "Auction",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Auction_SellerId",
                table: "Auction",
                column: "SellerId");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionBidRequest_AuctionId",
                table: "AuctionBidRequest",
                column: "AuctionId");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionBidRequest_BidderUserId",
                table: "AuctionBidRequest",
                column: "BidderUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuctionOrder_AuctionId",
                table: "AuctionOrder",
                column: "AuctionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuctionOrder_AuctionRequestId",
                table: "AuctionOrder",
                column: "AuctionRequestId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuctionOrder");

            migrationBuilder.DropTable(
                name: "AuctionBidRequest");

            migrationBuilder.DropTable(
                name: "Auction");
        }
    }
}
