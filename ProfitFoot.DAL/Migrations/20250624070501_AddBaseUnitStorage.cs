using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProfitFood.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseUnitStorage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BaseUnitStorageId",
                table: "Products",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "BaseUnitsStorage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BaseUnitsStorage", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_BaseUnitStorageId",
                table: "Products",
                column: "BaseUnitStorageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_BaseUnitsStorage_BaseUnitStorageId",
                table: "Products",
                column: "BaseUnitStorageId",
                principalTable: "BaseUnitsStorage",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_BaseUnitsStorage_BaseUnitStorageId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "BaseUnitsStorage");

            migrationBuilder.DropIndex(
                name: "IX_Products_BaseUnitStorageId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "BaseUnitStorageId",
                table: "Products");
        }
    }
}
