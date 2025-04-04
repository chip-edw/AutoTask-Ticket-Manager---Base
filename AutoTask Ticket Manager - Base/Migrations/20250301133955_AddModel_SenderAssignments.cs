using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoTaskTicketManager_Base.Migrations
{
    /// <inheritdoc />
    public partial class AddModel_SenderAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SenderAssignments",
                columns: table => new
                {
                    AT_Resource_Id = table.Column<string>(type: "TEXT", nullable: false),
                    Resource_Name = table.Column<string>(type: "TEXT", nullable: false),
                    Resource_Email = table.Column<string>(type: "TEXT", nullable: false),
                    Resource_Role = table.Column<string>(type: "TEXT", nullable: false),
                    Resource_Active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SenderAssignments", x => x.AT_Resource_Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SenderAssignments");
        }
    }
}
