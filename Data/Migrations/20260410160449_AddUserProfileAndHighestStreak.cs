using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FNaFle.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfileAndHighestStreak : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HighestStreak",
                table: "UserProgress",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfilePicturePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FavChar1Id = table.Column<int>(type: "int", nullable: true),
                    FavChar2Id = table.Column<int>(type: "int", nullable: true),
                    FavChar3Id = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserProfiles_Characters_FavChar1Id",
                        column: x => x.FavChar1Id,
                        principalTable: "Characters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserProfiles_Characters_FavChar2Id",
                        column: x => x.FavChar2Id,
                        principalTable: "Characters",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserProfiles_Characters_FavChar3Id",
                        column: x => x.FavChar3Id,
                        principalTable: "Characters",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_FavChar1Id",
                table: "UserProfiles",
                column: "FavChar1Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_FavChar2Id",
                table: "UserProfiles",
                column: "FavChar2Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_FavChar3Id",
                table: "UserProfiles",
                column: "FavChar3Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "HighestStreak",
                table: "UserProgress");
        }
    }
}
