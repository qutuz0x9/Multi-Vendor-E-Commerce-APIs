using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MultiVendorECommerce.Domain.Enums;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MultiVendorECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ImplementAddresses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:Category_status", "active,inactive")
                .Annotation("Npgsql:Enum:brand_status", "active,inactive")
                .Annotation("Npgsql:Enum:customer_address_type", "billing,pickup,shipping")
                .Annotation("Npgsql:Enum:inventory_status", "available,depleted")
                .Annotation("Npgsql:Enum:order_status", "cancelled,confirmed,delivered,pending,shipped")
                .Annotation("Npgsql:Enum:payment_status", "completed,failed,pending,refunded")
                .Annotation("Npgsql:Enum:product_status", "active,drafted,inactive")
                .Annotation("Npgsql:Enum:user_status", "active,banned,suspended")
                .Annotation("Npgsql:Enum:vendor_address_type", "pickup_point,return,warehouse")
                .Annotation("Npgsql:Enum:vendor_offer_status", "active,inactive,out_of_stock")
                .Annotation("Npgsql:Enum:vendor_status", "approved,pending,rejected,suspended")
                .OldAnnotation("Npgsql:Enum:Category_status", "active,inactive")
                .OldAnnotation("Npgsql:Enum:brand_status", "active,inactive")
                .OldAnnotation("Npgsql:Enum:inventory_status", "available,depleted")
                .OldAnnotation("Npgsql:Enum:order_status", "cancelled,confirmed,delivered,pending,shipped")
                .OldAnnotation("Npgsql:Enum:payment_status", "completed,failed,pending,refunded")
                .OldAnnotation("Npgsql:Enum:product_status", "active,drafted,inactive")
                .OldAnnotation("Npgsql:Enum:user_status", "active,banned,suspended")
                .OldAnnotation("Npgsql:Enum:vendor_offer_status", "active,inactive,out_of_stock")
                .OldAnnotation("Npgsql:Enum:vendor_status", "approved,pending,rejected,suspended");

            migrationBuilder.CreateTable(
                name: "CustomerAddress",
                schema: "customer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AddressType = table.Column<CustomerAddressType>(type: "customer_address_type", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerAddress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerAddress_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "customer",
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VendorAddress",
                schema: "vendor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    VendorId = table.Column<Guid>(type: "uuid", nullable: false),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AddressType = table.Column<VendorAddressType>(type: "vendor_address_type", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorAddress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendorAddress_Vendor_VendorId",
                        column: x => x.VendorId,
                        principalSchema: "vendor",
                        principalTable: "Vendor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAddress_AddressType",
                schema: "customer",
                table: "CustomerAddress",
                column: "AddressType");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerAddress_CustomerId",
                schema: "customer",
                table: "CustomerAddress",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorAddress_AddressType",
                schema: "vendor",
                table: "VendorAddress",
                column: "AddressType");

            migrationBuilder.CreateIndex(
                name: "IX_VendorAddress_VendorId",
                schema: "vendor",
                table: "VendorAddress",
                column: "VendorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerAddress",
                schema: "customer");

            migrationBuilder.DropTable(
                name: "VendorAddress",
                schema: "vendor");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:Category_status", "active,inactive")
                .Annotation("Npgsql:Enum:brand_status", "active,inactive")
                .Annotation("Npgsql:Enum:inventory_status", "available,depleted")
                .Annotation("Npgsql:Enum:order_status", "cancelled,confirmed,delivered,pending,shipped")
                .Annotation("Npgsql:Enum:payment_status", "completed,failed,pending,refunded")
                .Annotation("Npgsql:Enum:product_status", "active,drafted,inactive")
                .Annotation("Npgsql:Enum:user_status", "active,banned,suspended")
                .Annotation("Npgsql:Enum:vendor_offer_status", "active,inactive,out_of_stock")
                .Annotation("Npgsql:Enum:vendor_status", "approved,pending,rejected,suspended")
                .OldAnnotation("Npgsql:Enum:Category_status", "active,inactive")
                .OldAnnotation("Npgsql:Enum:brand_status", "active,inactive")
                .OldAnnotation("Npgsql:Enum:customer_address_type", "billing,pickup,shipping")
                .OldAnnotation("Npgsql:Enum:inventory_status", "available,depleted")
                .OldAnnotation("Npgsql:Enum:order_status", "cancelled,confirmed,delivered,pending,shipped")
                .OldAnnotation("Npgsql:Enum:payment_status", "completed,failed,pending,refunded")
                .OldAnnotation("Npgsql:Enum:product_status", "active,drafted,inactive")
                .OldAnnotation("Npgsql:Enum:user_status", "active,banned,suspended")
                .OldAnnotation("Npgsql:Enum:vendor_address_type", "pickup_point,return,warehouse")
                .OldAnnotation("Npgsql:Enum:vendor_offer_status", "active,inactive,out_of_stock")
                .OldAnnotation("Npgsql:Enum:vendor_status", "approved,pending,rejected,suspended");
        }
    }
}
