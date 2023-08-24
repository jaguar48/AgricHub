using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AgricHub.DAL.Migrations
{
    /// <inheritdoc />
    public partial class dbsets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "Wallets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WalletNo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ConsultantId = table.Column<int>(type: "int", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(38,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wallets_Consultants_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "Consultants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2ced457e-de90-4751-a8d2-900b0bb88d26", "33049767-1c68-4ba8-8d62-f00b5b9d8b89", "Consultant", "CONSULTANT" },
                    { "349e0fba-a928-4d9d-ae27-925d3f25a194", "6d912fa8-4047-446b-b696-549a0c05355b", "Admin", "ADMIN" },
                    { "798304ad-50f1-47e8-8338-9cfd1cc5a031", "ba133474-035e-4041-8877-655e39cb1c25", "Customer", "CUSTOMER" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_ConsultantId",
                table: "Wallets",
                column: "ConsultantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Wallets");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2ced457e-de90-4751-a8d2-900b0bb88d26");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "349e0fba-a928-4d9d-ae27-925d3f25a194");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "798304ad-50f1-47e8-8338-9cfd1cc5a031");

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
    }
}
