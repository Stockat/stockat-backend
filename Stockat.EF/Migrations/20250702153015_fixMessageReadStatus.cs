using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stockat.EF.Migrations
{
    /// <inheritdoc />
    public partial class fixMessageReadStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MessageReadStatuses",
                table: "MessageReadStatuses");

            migrationBuilder.DropIndex(
                name: "IX_MessageReadStatuses_MessageId",
                table: "MessageReadStatuses");

            migrationBuilder.DropIndex(
                name: "IX_MessageReadStatuses_UserId",
                table: "MessageReadStatuses");

            migrationBuilder.DropColumn(
                name: "ReadStatusId",
                table: "MessageReadStatuses");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessageReadStatuses",
                table: "MessageReadStatuses",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReadStatuses_UserId_MessageId",
                table: "MessageReadStatuses",
                columns: new[] { "UserId", "MessageId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_MessageReadStatuses",
                table: "MessageReadStatuses");

            migrationBuilder.DropIndex(
                name: "IX_MessageReadStatuses_UserId_MessageId",
                table: "MessageReadStatuses");

            migrationBuilder.AddColumn<int>(
                name: "ReadStatusId",
                table: "MessageReadStatuses",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MessageReadStatuses",
                table: "MessageReadStatuses",
                column: "ReadStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReadStatuses_MessageId",
                table: "MessageReadStatuses",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReadStatuses_UserId",
                table: "MessageReadStatuses",
                column: "UserId");
        }
    }
}
