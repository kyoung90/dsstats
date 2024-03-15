using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MysqlMigrations.Migrations
{
    /// <inheritdoc />
    public partial class PlayerRatingRatingNgType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RatingNgType",
                table: "ReplayPlayerNgRatings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PlayerNgRatingChanges",
                columns: table => new
                {
                    PlayerNgRatingChangeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Change24h = table.Column<float>(type: "float", nullable: false),
                    Change10d = table.Column<float>(type: "float", nullable: false),
                    Change30d = table.Column<float>(type: "float", nullable: false),
                    PlayerNgRatingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerNgRatingChanges", x => x.PlayerNgRatingChangeId);
                    table.ForeignKey(
                        name: "FK_PlayerNgRatingChanges_PlayerNgRatings_PlayerNgRatingId",
                        column: x => x.PlayerNgRatingId,
                        principalTable: "PlayerNgRatings",
                        principalColumn: "PlayerNgRatingId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerNgRatingChanges_PlayerNgRatingId",
                table: "PlayerNgRatingChanges",
                column: "PlayerNgRatingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerNgRatingChanges");

            migrationBuilder.DropColumn(
                name: "RatingNgType",
                table: "ReplayPlayerNgRatings");
        }
    }
}
