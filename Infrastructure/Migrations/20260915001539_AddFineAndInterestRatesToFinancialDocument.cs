using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFineAndInterestRatesToFinancialDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FineRate",
                table: "FinancialDocuments",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InterestDailyRate",
                table: "FinancialDocuments",
                type: "numeric(7,4)",
                precision: 7,
                scale: 4,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FineRate",
                table: "FinancialDocuments");

            migrationBuilder.DropColumn(
                name: "InterestDailyRate",
                table: "FinancialDocuments");
        }
    }
}
