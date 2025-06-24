using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfitFood.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BaseUnitId",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "BaseUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseUnits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_BaseUnitId",
                table: "Products",
                column: "BaseUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_BaseUnits_BaseUnitId",
                table: "Products",
                column: "BaseUnitId",
                principalTable: "BaseUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_BaseUnits_BaseUnitId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "BaseUnits");

            migrationBuilder.DropIndex(
                name: "IX_Products_BaseUnitId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BaseUnitId",
                table: "Products");
        }
    }
}
