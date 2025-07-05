using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Stockat.EF.Migrations
{
    /// <inheritdoc />
    public partial class chatEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Removed to avoid duplicate column error
            // migrationBuilder.AddColumn<string>(
            //     name: "AdditionalNote",
            //     table: "ServiceRequestUpdates",
            //     type: "nvarchar(500)",
            //     maxLength: 500,
            //     nullable: true);

            // migrationBuilder.AddColumn<string>(
            //     name: "AboutMe",
            //     table: "AspNetUsers",
            //     type: "nvarchar(max)",
            //     nullable: true);

            // migrationBuilder.AddColumn<string>(
            //     name: "Address",
            //     table: "AspNetUsers",
            //     type: "nvarchar(max)",
            //     nullable: true);

            // migrationBuilder.AddColumn<string>(
            //     name: "City",
            //     table: "AspNetUsers",
            //     type: "nvarchar(max)",
            //     nullable: true);

            // migrationBuilder.AddColumn<string>(
            //     name: "Country",
            //     table: "AspNetUsers",
            //     type: "nvarchar(max)",
            //     nullable: true);

            // migrationBuilder.AddColumn<bool>(
            //     name: "IsDeleted",
            //     table: "AspNetUsers",
            //     type: "bit",
            //     nullable: false,
            //     defaultValue: false);

            // migrationBuilder.AddColumn<string>(
            //     name: "PostalCode",
            //     table: "AspNetUsers",
            //     type: "nvarchar(max)",
            //     nullable: true);

            // migrationBuilder.AddColumn<string>(
            //     name: "ProfileImageId",
            //     table: "AspNetUsers",
            //     type: "nvarchar(max)",
            //     nullable: true);

            // migrationBuilder.AddColumn<string>(
            //     name: "ProfileImageUrl",
            //     table: "AspNetUsers",
            //     type: "nvarchar(max)",
            //     nullable: true);

            migrationBuilder.CreateTable(
                name: "ChatConversations",
                columns: table => new
                {
                    ConversationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User1Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    User2Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatConversations", x => x.ConversationId);
                    table.ForeignKey(
                        name: "FK_ChatConversations_AspNetUsers_User1Id",
                        column: x => x.User1Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ChatConversations_AspNetUsers_User2Id",
                        column: x => x.User2Id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    MessageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConversationId = table.Column<int>(type: "int", nullable: false),
                    SenderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MessageText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ImageId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VoiceUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VoiceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEdited = table.Column<bool>(type: "bit", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_ChatMessages_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatMessages_ChatConversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "ChatConversations",
                        principalColumn: "ConversationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessageReactions",
                columns: table => new
                {
                    ReactionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReactionType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageReactions", x => x.ReactionId);
                    table.ForeignKey(
                        name: "FK_MessageReactions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageReactions_ChatMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "ChatMessages",
                        principalColumn: "MessageId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MessageReadStatuses",
                columns: table => new
                {
                    ReadStatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageReadStatuses", x => x.ReadStatusId);
                    table.ForeignKey(
                        name: "FK_MessageReadStatuses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MessageReadStatuses_ChatMessages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "ChatMessages",
                        principalColumn: "MessageId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatConversations_User1Id",
                table: "ChatConversations",
                column: "User1Id");

            migrationBuilder.CreateIndex(
                name: "IX_ChatConversations_User2Id",
                table: "ChatConversations",
                column: "User2Id");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ConversationId",
                table: "ChatMessages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderId",
                table: "ChatMessages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_MessageId",
                table: "MessageReactions",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReactions_UserId",
                table: "MessageReactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReadStatuses_MessageId",
                table: "MessageReadStatuses",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageReadStatuses_UserId",
                table: "MessageReadStatuses",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessageReactions");

            migrationBuilder.DropTable(
                name: "MessageReadStatuses");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "ChatConversations");

            // Removed to match the Up method
            // migrationBuilder.DropColumn(
            //     name: "AdditionalNote",
            //     table: "ServiceRequestUpdates");

            // migrationBuilder.DropColumn(
            //     name: "AboutMe",
            //     table: "AspNetUsers");

            // migrationBuilder.DropColumn(
            //     name: "Address",
            //     table: "AspNetUsers");

            // migrationBuilder.DropColumn(
            //     name: "City",
            //     table: "AspNetUsers");

            // migrationBuilder.DropColumn(
            //     name: "Country",
            //     table: "AspNetUsers");

            // migrationBuilder.DropColumn(
            //     name: "IsDeleted",
            //     table: "AspNetUsers");

            // migrationBuilder.DropColumn(
            //     name: "PostalCode",
            //     table: "AspNetUsers");

            // migrationBuilder.DropColumn(
            //     name: "ProfileImageId",
            //     table: "AspNetUsers");

            // migrationBuilder.DropColumn(
            //     name: "ProfileImageUrl",
            //     table: "AspNetUsers");
        }
    }
}
