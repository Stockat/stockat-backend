using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stockat.EF.Migrations
{
    /// <inheritdoc />
    public partial class addfieldnoteserviceUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalNote",
                table: "ServiceRequestUpdates",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalNote",
                table: "ServiceRequestUpdates");
        }
    }
}
