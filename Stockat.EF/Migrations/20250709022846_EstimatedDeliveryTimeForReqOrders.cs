using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stockat.EF.Migrations
{
    /// <inheritdoc />
    public partial class EstimatedDeliveryTimeForReqOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedDeliveryTime",
                table: "OrderProduct",
                type: "datetime2",
                nullable: true);


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
     
           migrationBuilder.DropColumn(
                name: "EstimatedDeliveryTime",
                table: "OrderProduct");


        }
    }
}
