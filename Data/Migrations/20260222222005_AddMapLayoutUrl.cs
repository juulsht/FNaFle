using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FNaFle.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMapLayoutUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MapLayoutUrl",
                table: "MapLocations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MapLayoutUrl",
                table: "MapLocations");
        }
    }
}
