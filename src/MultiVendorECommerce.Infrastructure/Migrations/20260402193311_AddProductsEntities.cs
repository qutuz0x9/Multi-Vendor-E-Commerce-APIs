using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using MultiVendorECommerce.Domain.Enums;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MultiVendorECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductsEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "product");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:Category_status", "active,inactive")
                .Annotation("Npgsql:Enum:brand_status", "active,inactive")
                .Annotation("Npgsql:Enum:brand_status.brand_status", "active,inactive")
                .Annotation("Npgsql:Enum:category_status.category_status", "active,inactive")
                .Annotation("Npgsql:Enum:product_status", "active,drafted,inactive")
                .Annotation("Npgsql:Enum:product_status.product_status", "active,inactive,drafted")
                .Annotation("Npgsql:Enum:user_status", "active,banned,suspended")
                .Annotation("Npgsql:Enum:user_status.user_status", "active,suspended,banned")
                .Annotation("Npgsql:Enum:vendor_status", "approved,pending,rejected,suspended")
                .Annotation("Npgsql:Enum:vendor_status.vendor_status", "pending,approved,rejected,suspended")
                .OldAnnotation("Npgsql:Enum:user_status", "active,banned,suspended")
                .OldAnnotation("Npgsql:Enum:user_status.user_status", "active,suspended,banned")
                .OldAnnotation("Npgsql:Enum:vendor_status", "approved,pending,rejected,suspended")
                .OldAnnotation("Npgsql:Enum:vendor_status.vendor_status", "pending,approved,rejected,suspended");

            migrationBuilder.CreateTable(
                name: "Brand",
                schema: "product",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NormalizedName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    brand_status = table.Column<BrandStatus>(type: "brand_status", nullable: false, defaultValue: BrandStatus.Active),
                    Slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brand", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                schema: "product",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Slug = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Status = table.Column<CategoryStatus>(type: "\"Category_status\"", nullable: false, defaultValue: CategoryStatus.Active),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                schema: "product",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    BrandId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Feature = table.Column<JsonDocument>(type: "jsonb", nullable: true),
                    Status = table.Column<ProductStatus>(type: "product_status", nullable: true, defaultValue: ProductStatus.Active),
                    Slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Product_Brand_BrandId",
                        column: x => x.BrandId,
                        principalSchema: "product",
                        principalTable: "Brand",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductCategory",
                schema: "product",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    ProductId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductCategory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductCategory_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "product",
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductCategory_Product_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "product",
                        principalTable: "Product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Brand_IsDeleted",
                schema: "product",
                table: "Brand",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Brand_Name",
                schema: "product",
                table: "Brand",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Brand_NormalizedName",
                schema: "product",
                table: "Brand",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Brand_Slug",
                schema: "product",
                table: "Brand",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Category_IsDeleted",
                schema: "product",
                table: "Category",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Category_Name",
                schema: "product",
                table: "Category",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Category_Slug",
                schema: "product",
                table: "Category",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_BrandId",
                schema: "product",
                table: "Product",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Product_IsDeleted",
                schema: "product",
                table: "Product",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Name",
                schema: "product",
                table: "Product",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Product_Slug",
                schema: "product",
                table: "Product",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategory_CategoryId",
                schema: "product",
                table: "ProductCategory",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategory_ProductId",
                schema: "product",
                table: "ProductCategory",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategory_ProductId_CategoryId",
                schema: "product",
                table: "ProductCategory",
                columns: new[] { "ProductId", "CategoryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductCategory",
                schema: "product");

            migrationBuilder.DropTable(
                name: "Category",
                schema: "product");

            migrationBuilder.DropTable(
                name: "Product",
                schema: "product");

            migrationBuilder.DropTable(
                name: "Brand",
                schema: "product");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:user_status", "active,banned,suspended")
                .Annotation("Npgsql:Enum:user_status.user_status", "active,suspended,banned")
                .Annotation("Npgsql:Enum:vendor_status", "approved,pending,rejected,suspended")
                .Annotation("Npgsql:Enum:vendor_status.vendor_status", "pending,approved,rejected,suspended")
                .OldAnnotation("Npgsql:Enum:Category_status", "active,inactive")
                .OldAnnotation("Npgsql:Enum:brand_status", "active,inactive")
                .OldAnnotation("Npgsql:Enum:brand_status.brand_status", "active,inactive")
                .OldAnnotation("Npgsql:Enum:category_status.category_status", "active,inactive")
                .OldAnnotation("Npgsql:Enum:product_status", "active,drafted,inactive")
                .OldAnnotation("Npgsql:Enum:product_status.product_status", "active,inactive,drafted")
                .OldAnnotation("Npgsql:Enum:user_status", "active,banned,suspended")
                .OldAnnotation("Npgsql:Enum:user_status.user_status", "active,suspended,banned")
                .OldAnnotation("Npgsql:Enum:vendor_status", "approved,pending,rejected,suspended")
                .OldAnnotation("Npgsql:Enum:vendor_status.vendor_status", "pending,approved,rejected,suspended");
        }
    }
}
