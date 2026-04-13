using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MultiVendorECommerce.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToVendorModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vendor_User_Id",
                schema: "vendor",
                table: "Vendor");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "vendor",
                table: "Vendor",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Vendor_UserId",
                schema: "vendor",
                table: "Vendor",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Vendor_User_UserId",
                schema: "vendor",
                table: "Vendor",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Vendor_User_UserId",
                schema: "vendor",
                table: "Vendor");

            migrationBuilder.DropIndex(
                name: "IX_Vendor_UserId",
                schema: "vendor",
                table: "Vendor");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "vendor",
                table: "Vendor");

            migrationBuilder.AddForeignKey(
                name: "FK_Vendor_User_Id",
                schema: "vendor",
                table: "Vendor",
                column: "Id",
                principalSchema: "identity",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
