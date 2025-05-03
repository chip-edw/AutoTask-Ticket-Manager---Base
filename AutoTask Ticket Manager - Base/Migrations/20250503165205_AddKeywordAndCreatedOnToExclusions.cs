using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoTaskTicketManager_Base.Migrations
{
    /// <inheritdoc />
    public partial class AddKeywordAndCreatedOnToExclusions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SubjectKeyWord",
                table: "SubjectExclusionKeywords",
                newName: "KeyWord");

            migrationBuilder.RenameColumn(
                name: "SenderAddress",
                table: "SenderExclusions",
                newName: "Email");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "SubjectExclusionKeywords",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                table: "SenderExclusions",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "SubjectExclusionKeywords");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                table: "SenderExclusions");

            migrationBuilder.RenameColumn(
                name: "KeyWord",
                table: "SubjectExclusionKeywords",
                newName: "SubjectKeyWord");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "SenderExclusions",
                newName: "SenderAddress");
        }
    }
}
