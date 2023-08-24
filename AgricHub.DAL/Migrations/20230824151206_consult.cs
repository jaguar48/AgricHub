using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AgricHub.DAL.Migrations
{
    /// <inheritdoc />
    public partial class consult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "23981e2b-8f38-4e5e-8937-8505b698f246");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "410f2605-a5c8-46c5-889d-fb2a6cecf13b");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "eedf4b5d-1d74-4853-8211-02a8d3f2f0a6");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "022618f0-40e1-4928-8f64-1528a6e50677", "acc8e2a6-b2bd-4ea3-9434-318ce0e42bbc", "Customer", "CUSTOMER" },
                    { "121c6379-7a31-43cb-9663-54e976cc1508", "fb1b4f4a-a3a2-4dfb-ae0d-7934cb5de5de", "Admin", "ADMIN" },
                    { "26e31bd6-11ae-4933-8c31-47d0ba68c652", "984657f8-ce60-48de-877c-64eba4007e30", "Consultant", "CONSULTANT" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "022618f0-40e1-4928-8f64-1528a6e50677");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "121c6379-7a31-43cb-9663-54e976cc1508");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "26e31bd6-11ae-4933-8c31-47d0ba68c652");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "23981e2b-8f38-4e5e-8937-8505b698f246", "87c64678-0d5c-4e9e-9b62-1bcb56af7120", "Customer", "CUSTOMER" },
                    { "410f2605-a5c8-46c5-889d-fb2a6cecf13b", "6857eff6-e74a-4ccd-8b1c-fe61b4434618", "Admin", "ADMIN" },
                    { "eedf4b5d-1d74-4853-8211-02a8d3f2f0a6", "ff8f972d-36d3-41dd-a749-6217b880974a", "Consultant", "CONSULTANT" }
                });
        }
    }
}
