using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stockat.EF.Migrations
{
    /// <inheritdoc />
    public partial class removeredundantfieldserviceUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerApprovalStatus",
                table: "ServiceRequestUpdates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BuyerApprovalStatus",
                table: "ServiceRequestUpdates",
                type: "int",
                maxLength: 20,
                nullable: false,
                defaultValue: 0);
        }
    }
}
