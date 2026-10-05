using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EasyUpdate.Migrations
{
    /// <inheritdoc />
    public partial class AddDeployAndRollbackScripts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ServerPath",
                table: "SoftwareApps",
                newName: "AppFolderPath");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AppFolderPath",
                table: "SoftwareApps",
                newName: "ServerPath");
        }
    }
}
