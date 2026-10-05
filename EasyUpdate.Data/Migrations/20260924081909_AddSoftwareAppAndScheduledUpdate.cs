using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyUpdate.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftwareAppAndScheduledUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SoftwareApps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServerPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeployScriptPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RollbackScriptPath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentVersion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SoftwareApps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledUpdates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SoftwareAppId = table.Column<int>(type: "int", nullable: false),
                    TargetVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PackagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScheduledStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogOutput = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScheduledByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledUpdates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledUpdates_AspNetUsers_ScheduledByUserId",
                        column: x => x.ScheduledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduledUpdates_SoftwareApps_SoftwareAppId",
                        column: x => x.SoftwareAppId,
                        principalTable: "SoftwareApps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledUpdates_ScheduledByUserId",
                table: "ScheduledUpdates",
                column: "ScheduledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledUpdates_SoftwareAppId",
                table: "ScheduledUpdates",
                column: "SoftwareAppId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledUpdates");

            migrationBuilder.DropTable(
                name: "SoftwareApps");
        }
    }
}
