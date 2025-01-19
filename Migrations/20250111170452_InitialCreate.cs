using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoTaskTicketManager_Base.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfigStore",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ValueName = table.Column<string>(type: "TEXT", nullable: false),
                    ValueType = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigStore", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerSettings",
                columns: table => new
                {
                    AutotaskId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AccountName = table.Column<string>(type: "TEXT", nullable: false),
                    SupportEmail = table.Column<string>(type: "TEXT", nullable: true),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableEmail = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoAssign = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerSettings", x => x.AutotaskId);
                });

            migrationBuilder.CreateTable(
                name: "Schedulers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TaskID = table.Column<int>(type: "INTEGER", nullable: false),
                    TaskName = table.Column<string>(type: "TEXT", nullable: false),
                    Period = table.Column<string>(type: "TEXT", nullable: false),
                    FrequencyMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    LastRunTime = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('0001-01-01 00:00:00')"),
                    NextRunTime = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "datetime('9999-12-31 23:59:59')"),
                    TaskActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedulers", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigStore");

            migrationBuilder.DropTable(
                name: "CustomerSettings");

            migrationBuilder.DropTable(
                name: "Schedulers");
        }
    }
}
