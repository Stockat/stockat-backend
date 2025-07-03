using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stockat.EF.Migrations
{
    /// <inheritdoc />
    public partial class chatEnities2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageReadStatuses_ChatMessages_MessageId",
                table: "MessageReadStatuses");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReadStatuses_ChatMessages_MessageId",
                table: "MessageReadStatuses",
                column: "MessageId",
                principalTable: "ChatMessages",
                principalColumn: "MessageId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MessageReadStatuses_ChatMessages_MessageId",
                table: "MessageReadStatuses");

            migrationBuilder.AddForeignKey(
                name: "FK_MessageReadStatuses_ChatMessages_MessageId",
                table: "MessageReadStatuses",
                column: "MessageId",
                principalTable: "ChatMessages",
                principalColumn: "MessageId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
