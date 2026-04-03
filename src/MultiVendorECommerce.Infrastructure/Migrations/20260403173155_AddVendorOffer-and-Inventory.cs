using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MultiVendorECommerce.Domain.Enums;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MultiVendorECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorOfferandInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "inventory");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:Category_status", "active,inactive")
                .Annotation("Npgsql:Enum:brand_status", "active,inactive")
                .Annotation("Npgsql:Enum:inventory_status", "available,depleted")
                .Annotation("Npgsql:Enum:product_status", "active,drafted,inactive")
                .Annotation("Npgsql:Enum:user_status", "active,banned,suspended")
                .Annotation("Npgsql:Enum:vendor_offer_status", "active,inactive,out_of_stock")
                .Annotation("Npgsql:Enum:vendor_status", "approved,pending,rejected,suspended")
                .OldAnnotation("Npgsql:Enum:Category_status", "active,inactive")
                .OldAnnotation("Npgsql:Enum:brand_status", "active,inactive")
                .OldAnnotation("Npgsql:Enum:product_status", "active,drafted,inactive")
                .OldAnnotation("Npgsql:Enum:user_status", "active,banned,suspended")
                .OldAnnotation("Npgsql:Enum:vendor_status", "approved,pending,rejected,suspended");

            migrationBuilder.CreateTable(
                name: "VendorOffer",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    VendorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Staus = table.Column<VendorOfferStatus>(type: "vendor_offer_status", nullable: false, defaultValue: VendorOfferStatus.Active),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorOffer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorOffer_Product_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "product",
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VendorOffer_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalSchema: "vendor",
                        principalTable: "Vendor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inventory",
                schema: "inventory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    VendorOfferId = table.Column<int>(type: "integer", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ReservedQuantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Status = table.Column<InventoryStatus>(type: "inventory_status", nullable: false, defaultValue: InventoryStatus.Available),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventory_VendorOffer_VendorOfferId",
                        column: x => x.VendorOfferId,
                        principalSchema: "inventory",
                        principalTable: "VendorOffer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_IsDeleted",
                schema: "inventory",
                table: "Inventory",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Inventory_VendorOfferId",
                schema: "inventory",
                table: "Inventory",
                column: "VendorOfferId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorOffer_IsDeleted",
                schema: "inventory",
                table: "VendorOffer",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_VendorOffer_ProductId",
                schema: "inventory",
                table: "VendorOffer",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorOffer_VendorId_ProductId",
                schema: "inventory",
                table: "VendorOffer",
                columns: new[] { "VendorId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inventory",
                schema: "inventory");

            migrationBuilder.DropTable(
                name: "VendorOffer",
                schema: "inventory");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:Category_status", "active,inactive")
                .Annotation("Npgsql:Enum:brand_status", "active,inactive")
                .Annotation("Npgsql:Enum:product_status", "active,drafted,inactive")
                .Annotation("Npgsql:Enum:user_status", "active,banned,suspended")
                .Annotation("Npgsql:Enum:vendor_status", "approved,pending,rejected,suspended")
                .OldAnnotation("Npgsql:Enum:Category_status", "active,inactive")
                .OldAnnotation("Npgsql:Enum:brand_status", "active,inactive")
                .OldAnnotation("Npgsql:Enum:inventory_status", "available,depleted")
                .OldAnnotation("Npgsql:Enum:product_status", "active,drafted,inactive")
                .OldAnnotation("Npgsql:Enum:user_status", "active,banned,suspended")
                .OldAnnotation("Npgsql:Enum:vendor_offer_status", "active,inactive,out_of_stock")
                .OldAnnotation("Npgsql:Enum:vendor_status", "approved,pending,rejected,suspended");
        }
    }
}
