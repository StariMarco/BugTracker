using Microsoft.EntityFrameworkCore.Migrations;

namespace BugTracker.Migrations
{
    public partial class addCharLimitToCommentText : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Comment",
                table: "TicketComments");

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "TicketComments",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Text",
                table: "TicketComments");

            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "TicketComments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
