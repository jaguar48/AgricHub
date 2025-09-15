using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgricHub.DAL.Migrations
{
    /// <inheritdoc />
    public partial class package : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ServiceId",
                table: "Consultations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServicePackageId",
                table: "Consultations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ServicePackage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    PackageName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IncludesOnsiteVisit = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicePackage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicePackage_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Consultations_ServicePackageId",
                table: "Consultations",
                column: "ServicePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicePackage_ServiceId",
                table: "ServicePackage",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Consultations_ServicePackage_ServicePackageId",
                table: "Consultations",
                column: "ServicePackageId",
                principalTable: "ServicePackage",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Consultations_ServicePackage_ServicePackageId",
                table: "Consultations");

            migrationBuilder.DropTable(
                name: "ServicePackage");

            migrationBuilder.DropIndex(
                name: "IX_Consultations_ServicePackageId",
                table: "Consultations");

            migrationBuilder.DropColumn(
                name: "ServicePackageId",
                table: "Consultations");

            migrationBuilder.AlterColumn<int>(
                name: "ServiceId",
                table: "Consultations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
