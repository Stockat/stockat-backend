using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stockat.EF.Migrations
{
    /// <inheritdoc />
    public partial class ServiceEditRequestEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServiceEditRequest",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    EditedName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EditedDescription = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EditedMinQuantity = table.Column<int>(type: "int", nullable: false),
                    EditedPricePerProduct = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EditedEstimatedTime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EditedImageId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    EditedImageUrl = table.Column<string>(type: "nvarchar(2083)", maxLength: 2083, nullable: false),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdminNote = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceEditRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceEditRequest_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceEditRequest_ServiceId",
                table: "ServiceEditRequest",
                column: "ServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServiceEditRequest");
        }
    }
}
