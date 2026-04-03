using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MultiVendorECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCartFunctionality : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "cart");

            migrationBuilder.CreateTable(
                name: "CartSession",
                schema: "cart",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartSession", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartSession_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalSchema: "customer",
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartItem",
                schema: "cart",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CartSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    VendorOfferId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItem_CartSession_CartSessionId",
                        column: x => x.CartSessionId,
                        principalSchema: "cart",
                        principalTable: "CartSession",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItem_VendorOffer_VendorOfferId",
                        column: x => x.VendorOfferId,
                        principalSchema: "inventory",
                        principalTable: "VendorOffer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CartItem_CartSessionId",
                schema: "cart",
                table: "CartItem",
                column: "CartSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItem_CreatedAt",
                schema: "cart",
                table: "CartItem",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CartItem_VendorOfferId",
                schema: "cart",
                table: "CartItem",
                column: "VendorOfferId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CartSession_CustomerId",
                schema: "cart",
                table: "CartSession",
                column: "CustomerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CartItem",
                schema: "cart");

            migrationBuilder.DropTable(
                name: "CartSession",
                schema: "cart");
        }
    }
}
