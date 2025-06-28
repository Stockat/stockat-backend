using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stockat.EF.Migrations
{
    /// <inheritdoc />
    public partial class addMinQuantityColumnInProductTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinQuantity",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinQuantity",
                table: "Products");
        }
    }
}
