using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameFullnameToFullName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Fullname",
                table: "Users",
                newName: "FullName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "Users",
                newName: "Fullname");
        }
    }
}
