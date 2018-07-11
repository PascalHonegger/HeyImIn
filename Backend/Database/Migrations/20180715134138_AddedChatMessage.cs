using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace HeyImIn.Database.Migrations
{
    public partial class AddedChatMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastReadMessageId",
                table: "EventParticipations",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AuthorId = table.Column<int>(nullable: false),
                    EventId = table.Column<int>(nullable: false),
                    SentDate = table.Column<DateTime>(nullable: false),
                    Content = table.Column<string>(maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventParticipations_LastReadMessageId",
                table: "EventParticipations",
                column: "LastReadMessageId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_AuthorId",
                table: "ChatMessages",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_EventId",
                table: "ChatMessages",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SentDate",
                table: "ChatMessages",
                column: "SentDate");

            migrationBuilder.AddForeignKey(
                name: "FK_EventParticipations_ChatMessages_LastReadMessageId",
                table: "EventParticipations",
                column: "LastReadMessageId",
                principalTable: "ChatMessages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventParticipations_ChatMessages_LastReadMessageId",
                table: "EventParticipations");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_EventParticipations_LastReadMessageId",
                table: "EventParticipations");

            migrationBuilder.DropColumn(
                name: "LastReadMessageId",
                table: "EventParticipations");
        }
    }
}
