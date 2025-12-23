using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionTypeToExpenses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Expenses",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Expenses");
        }
    }
}
