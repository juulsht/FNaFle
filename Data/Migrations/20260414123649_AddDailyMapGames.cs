using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FNaFle.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyMapGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DailyMapGames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MapLocationId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyMapGames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyMapGames_MapLocations_MapLocationId",
                        column: x => x.MapLocationId,
                        principalTable: "MapLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyMapGames_MapLocationId",
                table: "DailyMapGames",
                column: "MapLocationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DailyMapGames");
        }
    }
}
