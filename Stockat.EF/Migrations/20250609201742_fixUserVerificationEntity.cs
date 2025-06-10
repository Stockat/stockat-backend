using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stockat.EF.Migrations
{
    /// <inheritdoc />
    public partial class fixUserVerificationEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserVerification",
                table: "UserVerification");

            migrationBuilder.DropIndex(
                name: "IX_UserVerification_UserId",
                table: "UserVerification");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "UserVerification");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "UserVerification",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserVerification",
                table: "UserVerification",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserVerification",
                table: "UserVerification");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "UserVerification",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "UserVerification",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserVerification",
                table: "UserVerification",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserVerification_UserId",
                table: "UserVerification",
                column: "UserId",
                unique: true);
        }
    }
}
