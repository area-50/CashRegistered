using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropInventoryRequisitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryRequisitionItems");

            migrationBuilder.DropTable(
                name: "InventoryRequisitions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventoryRequisitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FulfilledAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    OriginModule = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RequestedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryRequisitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InventoryRequisitionItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    InventoryRequisitionId = table.Column<int>(type: "integer", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryRequisitionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryRequisitionItems_InventoryRequisitions_InventoryRe~",
                        column: x => x.InventoryRequisitionId,
                        principalTable: "InventoryRequisitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryRequisitionItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryRequisitionItems_InventoryRequisitionId",
                table: "InventoryRequisitionItems",
                column: "InventoryRequisitionId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryRequisitionItems_ProductId",
                table: "InventoryRequisitionItems",
                column: "ProductId");
        }
    }
}
