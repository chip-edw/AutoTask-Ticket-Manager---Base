using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoTaskTicketManager_Base.Migrations
{
    /// <inheritdoc />
    public partial class DropNameFromSenderExclusions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenderName",
                table: "SenderExclusions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SenderName",
                table: "SenderExclusions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
