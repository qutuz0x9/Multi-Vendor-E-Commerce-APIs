using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiVendorECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEnumSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:Category_status", "active,inactive")
                .Annotation("Npgsql:Enum:brand_status", "active,inactive")
                .Annotation("Npgsql:Enum:product_status", "active,drafted,inactive")
                .Annotation("Npgsql:Enum:user_status", "active,banned,suspended")
                .Annotation("Npgsql:Enum:vendor_status", "approved,pending,rejected,suspended")
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                .OldAnnotation("Npgsql:Enum:Category_status", "active,inactive")
                .OldAnnotation("Npgsql:Enum:brand_status", "active,inactive")
                .OldAnnotation("Npgsql:Enum:product_status", "active,drafted,inactive")
                .OldAnnotation("Npgsql:Enum:user_status", "active,banned,suspended")
                .OldAnnotation("Npgsql:Enum:vendor_status", "approved,pending,rejected,suspended");
        }
    }
}
