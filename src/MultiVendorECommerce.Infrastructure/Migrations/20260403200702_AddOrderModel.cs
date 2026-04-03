using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MultiVendorECommerce.Domain.Enums;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MultiVendorECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "order");

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
                .OldAnnotation("Npgsql:Enum:inventory_status", "available,depleted")
                .OldAnnotation("Npgsql:Enum:product_status", "active,drafted,inactive")
                .OldAnnotation("Npgsql:Enum:user_status", "active,banned,suspended")
                .OldAnnotation("Npgsql:Enum:vendor_offer_status", "active,inactive,out_of_stock")
                .OldAnnotation("Npgsql:Enum:vendor_status", "approved,pending,rejected,suspended");

            migrationBuilder.CreateTable(
                name: "Order",
                schema: "order",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<OrderStatus>(type: "order_status", nullable: false, defaultValue: OrderStatus.Pending),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Order", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Order_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "customer",
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItem",
                schema: "order",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    VendorOfferId = table.Column<int>(type: "integer", nullable: false),
                    ProductName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItem_Order_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "order",
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItem_VendorOffer_VendorOfferId",
                        column: x => x.VendorOfferId,
                        principalSchema: "inventory",
                        principalTable: "VendorOffer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderShippingAddress",
                schema: "order",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    ShippingAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ShippingCity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShippingCountry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShippingPhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderShippingAddress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderShippingAddress_Order_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "order",
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                schema: "order",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<PaymentStatus>(type: "payment_status", nullable: false, defaultValue: PaymentStatus.Pending),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payment_Order_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "order",
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Order_CustomerId",
                schema: "order",
                table: "Order",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_Status",
                schema: "order",
                table: "Order",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_OrderId",
                schema: "order",
                table: "OrderItem",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_VendorOfferId",
                schema: "order",
                table: "OrderItem",
                column: "VendorOfferId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderShippingAddress_OrderId",
                schema: "order",
                table: "OrderShippingAddress",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payment_OrderId",
                schema: "order",
                table: "Payment",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payment_Status",
                schema: "order",
                table: "Payment",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderItem",
                schema: "order");

            migrationBuilder.DropTable(
                name: "OrderShippingAddress",
                schema: "order");

            migrationBuilder.DropTable(
                name: "Payment",
                schema: "order");

            migrationBuilder.DropTable(
                name: "Order",
                schema: "order");

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
                .OldAnnotation("Npgsql:Enum:inventory_status", "available,depleted")
                .OldAnnotation("Npgsql:Enum:order_status", "cancelled,confirmed,delivered,pending,shipped")
                .OldAnnotation("Npgsql:Enum:payment_status", "completed,failed,pending,refunded")
                .OldAnnotation("Npgsql:Enum:product_status", "active,drafted,inactive")
                .OldAnnotation("Npgsql:Enum:user_status", "active,banned,suspended")
                .OldAnnotation("Npgsql:Enum:vendor_offer_status", "active,inactive,out_of_stock")
                .OldAnnotation("Npgsql:Enum:vendor_status", "approved,pending,rejected,suspended");
        }
    }
}
