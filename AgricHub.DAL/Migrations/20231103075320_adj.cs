using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AgricHub.DAL.Migrations
{
    /// <inheritdoc />
    public partial class adj : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultants_AspNetUsers_UserId",
                table: "Consultants");

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

            migrationBuilder.DropColumn(
                name: "LgaId",
                table: "Consultants");

            migrationBuilder.RenameColumn(
                name: "LgaId",
                table: "AspNetUsers",
                newName: "Address");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Consultants",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "StateId",
                table: "Consultants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Consultants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CountryId",
                table: "Consultants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Consultants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConsultantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_categories_Consultants_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "Consultants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Businesses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ConsultantId = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Businesses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Businesses_Consultants_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "Consultants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Businesses_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessReview",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BusinessId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BusinessId1 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessReview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessReview_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BusinessReview_Businesses_BusinessId1",
                        column: x => x.BusinessId1,
                        principalTable: "Businesses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "48d2fb35-1623-4446-8a9b-a8cf399348cc", "6717adf9-b966-4310-8269-81e032ef825f", "Consultant", "CONSULTANT" },
                    { "bde1c1c4-1931-449f-98a6-61978b8a7b73", "b7e35e37-6189-487f-bd2c-3674b16c4877", "Admin", "ADMIN" },
                    { "c5bd26da-1571-471e-981f-f251d23cdea5", "ece21bce-a81a-4095-9f15-52487555f4e6", "Customer", "CUSTOMER" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_CategoryId",
                table: "Businesses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Businesses_ConsultantId",
                table: "Businesses",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessReview_BusinessId1",
                table: "BusinessReview",
                column: "BusinessId1");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessReview_UserId",
                table: "BusinessReview",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_categories_ConsultantId",
                table: "categories",
                column: "ConsultantId");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultants_AspNetUsers_UserId",
                table: "Consultants",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultants_AspNetUsers_UserId",
                table: "Consultants");

            migrationBuilder.DropTable(
                name: "BusinessReview");

            migrationBuilder.DropTable(
                name: "Businesses");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "48d2fb35-1623-4446-8a9b-a8cf399348cc");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "bde1c1c4-1931-449f-98a6-61978b8a7b73");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "c5bd26da-1571-471e-981f-f251d23cdea5");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Consultants");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "AspNetUsers",
                newName: "LgaId");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Consultants",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StateId",
                table: "Consultants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Consultants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CountryId",
                table: "Consultants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LgaId",
                table: "Consultants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "2ced457e-de90-4751-a8d2-900b0bb88d26", "33049767-1c68-4ba8-8d62-f00b5b9d8b89", "Consultant", "CONSULTANT" },
                    { "349e0fba-a928-4d9d-ae27-925d3f25a194", "6d912fa8-4047-446b-b696-549a0c05355b", "Admin", "ADMIN" },
                    { "798304ad-50f1-47e8-8338-9cfd1cc5a031", "ba133474-035e-4041-8877-655e39cb1c25", "Customer", "CUSTOMER" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_Consultants_AspNetUsers_UserId",
                table: "Consultants",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
