using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AgricHub.DAL.Migrations
{
    /// <inheritdoc />
    public partial class mod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessReview_Businesses_BusinessId1",
                table: "BusinessReview");

            migrationBuilder.DropIndex(
                name: "IX_BusinessReview_BusinessId1",
                table: "BusinessReview");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "8ead2d22-864b-42d6-861d-dec33bc1d135");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "94984f8a-1190-4eb2-8f1d-1ab3e902917f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e70c8dbe-dde9-4be7-b025-53d7e250e561");

            migrationBuilder.DropColumn(
                name: "BusinessId1",
                table: "BusinessReview");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Businesses");

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Consultants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "BusinessId",
                table: "BusinessReview",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "832d2d20-9404-4404-9a17-2dae03f71f4b", "fa0b0c20-e958-4492-b4d0-5656a961a458", "Admin", "ADMIN" },
                    { "94cce2ed-a6c6-4c65-9ba1-427cc68e320f", "01225b63-37a6-40f0-9480-4affed95319a", "Customer", "CUSTOMER" },
                    { "a6bb5a03-36a4-45ca-bbc9-e8264111fbb4", "c49bc897-ab98-4d09-8c04-93fc80f7751a", "Consultant", "CONSULTANT" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessReview_BusinessId",
                table: "BusinessReview",
                column: "BusinessId");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessReview_Businesses_BusinessId",
                table: "BusinessReview",
                column: "BusinessId",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessReview_Businesses_BusinessId",
                table: "BusinessReview");

            migrationBuilder.DropIndex(
                name: "IX_BusinessReview_BusinessId",
                table: "BusinessReview");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "832d2d20-9404-4404-9a17-2dae03f71f4b");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "94cce2ed-a6c6-4c65-9ba1-427cc68e320f");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a6bb5a03-36a4-45ca-bbc9-e8264111fbb4");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Consultants");

            migrationBuilder.AlterColumn<string>(
                name: "BusinessId",
                table: "BusinessReview",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "BusinessId1",
                table: "BusinessReview",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Businesses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "8ead2d22-864b-42d6-861d-dec33bc1d135", "5b6bb1a1-0e01-4a3d-9893-b0d182b79f36", "Consultant", "CONSULTANT" },
                    { "94984f8a-1190-4eb2-8f1d-1ab3e902917f", "61aad74b-c3b9-4a60-9683-3d6092d40111", "Admin", "ADMIN" },
                    { "e70c8dbe-dde9-4be7-b025-53d7e250e561", "7014e78f-be10-4e83-a3b4-8f171d8baf07", "Customer", "CUSTOMER" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessReview_BusinessId1",
                table: "BusinessReview",
                column: "BusinessId1");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessReview_Businesses_BusinessId1",
                table: "BusinessReview",
                column: "BusinessId1",
                principalTable: "Businesses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
