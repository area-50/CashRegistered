using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChartOfAccountsSeedAndStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "IsSynthetic",
                table: "ChartOfAccounts",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ChartOfAccounts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP AT TIME ZONE 'UTC'",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<bool>(
                name: "AllowPosting",
                table: "ChartOfAccounts",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.InsertData(
                table: "ChartOfAccounts",
                columns: new[] { "Id", "AccountType", "Code", "CreatedAt", "IsActive", "IsSynthetic", "Name", "Nature", "ParentAccountId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, "Asset", "1", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "ATIVO", "Debit", null, null },
                    { 2, "Liability", "2", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, true, "PASSIVO", "Credit", null, null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ChartOfAccounts",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ChartOfAccounts",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.AlterColumn<bool>(
                name: "IsSynthetic",
                table: "ChartOfAccounts",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ChartOfAccounts",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");

            migrationBuilder.AlterColumn<bool>(
                name: "AllowPosting",
                table: "ChartOfAccounts",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);
        }
    }
}
