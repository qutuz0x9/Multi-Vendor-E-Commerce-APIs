using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MultiVendorECommerce.Domain.Enums;

#nullable disable

namespace MultiVendorECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVendorModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "vendor");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:user_status", "active,banned,suspended")
                .Annotation("Npgsql:Enum:user_status.user_status", "active,suspended,banned")
                .Annotation("Npgsql:Enum:vendor_status", "approved,pending,rejected,suspended")
                .Annotation("Npgsql:Enum:vendor_status.vendor_status", "pending,approved,rejected,suspended")
                .OldAnnotation("Npgsql:Enum:user_status", "active,banned,suspended")
                .OldAnnotation("Npgsql:Enum:user_status.user_status", "active,suspended,banned");

            migrationBuilder.CreateTable(
                name: "Vendor",
                schema: "vendor",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    BusinessName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    WebsiteUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Status = table.Column<VendorStatus>(type: "vendor_status", nullable: false, defaultValue: VendorStatus.Pending),
                    AverageRate = table.Column<decimal>(type: "numeric(3,2)", nullable: true),
                    TotalReviews = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    Slug = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vendor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vendor_User_Id",
                        column: x => x.Id,
                        principalSchema: "identity",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_BusinessName",
                schema: "vendor",
                table: "Vendor",
                column: "BusinessName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_IsDeleted",
                schema: "vendor",
                table: "Vendor",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_Slug",
                schema: "vendor",
                table: "Vendor",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_WebsiteUrl",
                schema: "vendor",
                table: "Vendor",
                column: "WebsiteUrl",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Vendor",
                schema: "vendor");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:user_status", "active,banned,suspended")
                .Annotation("Npgsql:Enum:user_status.user_status", "active,suspended,banned")
                .OldAnnotation("Npgsql:Enum:user_status", "active,banned,suspended")
                .OldAnnotation("Npgsql:Enum:user_status.user_status", "active,suspended,banned")
                .OldAnnotation("Npgsql:Enum:vendor_status", "approved,pending,rejected,suspended")
                .OldAnnotation("Npgsql:Enum:vendor_status.vendor_status", "pending,approved,rejected,suspended");
        }
    }
}
