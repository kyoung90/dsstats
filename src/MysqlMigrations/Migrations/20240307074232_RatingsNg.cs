using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MysqlMigrations.Migrations
{
    /// <inheritdoc />
    public partial class RatingsNg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ArcadeReplays_ReplayHash",
                table: "ArcadeReplays");

            migrationBuilder.DropColumn(
                name: "ReplayHash",
                table: "ArcadeReplays");

            migrationBuilder.AddColumn<int>(
                name: "ReplayId",
                table: "ArcadeReplays",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlayerNgRatings",
                columns: table => new
                {
                    PlayerNgRatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RatingNgType = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<double>(type: "double", nullable: false),
                    Games = table.Column<int>(type: "int", nullable: false),
                    Wins = table.Column<int>(type: "int", nullable: false),
                    Consistency = table.Column<double>(type: "double", nullable: false),
                    Confidence = table.Column<double>(type: "double", nullable: false),
                    Pos = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Mvp = table.Column<int>(type: "int", nullable: false),
                    MainCount = table.Column<int>(type: "int", nullable: false),
                    MainCmdr = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerNgRatings", x => x.PlayerNgRatingId);
                    table.ForeignKey(
                        name: "FK_PlayerNgRatings_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ReplayNgRatings",
                columns: table => new
                {
                    ReplayNgRatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RatingNgType = table.Column<int>(type: "int", nullable: false),
                    LeaverType = table.Column<int>(type: "int", nullable: false),
                    Exp2Win = table.Column<float>(type: "float", nullable: false),
                    AvgRating = table.Column<int>(type: "int", nullable: false),
                    IsPreRating = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ReplayId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplayNgRatings", x => x.ReplayNgRatingId);
                    table.ForeignKey(
                        name: "FK_ReplayNgRatings_Replays_ReplayId",
                        column: x => x.ReplayId,
                        principalTable: "Replays",
                        principalColumn: "ReplayId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ReplayPlayerNgRatings",
                columns: table => new
                {
                    ReplayPlayerNgRatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Rating = table.Column<float>(type: "float", nullable: false),
                    Change = table.Column<float>(type: "float", nullable: false),
                    Games = table.Column<int>(type: "int", nullable: false),
                    Consistency = table.Column<float>(type: "float", nullable: false),
                    Confidence = table.Column<float>(type: "float", nullable: false),
                    ReplayPlayerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplayPlayerNgRatings", x => x.ReplayPlayerNgRatingId);
                    table.ForeignKey(
                        name: "FK_ReplayPlayerNgRatings_ReplayPlayers_ReplayPlayerId",
                        column: x => x.ReplayPlayerId,
                        principalTable: "ReplayPlayers",
                        principalColumn: "ReplayPlayerId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ArcadeReplays_ReplayId",
                table: "ArcadeReplays",
                column: "ReplayId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerNgRatings_PlayerId",
                table: "PlayerNgRatings",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReplayNgRatings_ReplayId",
                table: "ReplayNgRatings",
                column: "ReplayId");

            migrationBuilder.CreateIndex(
                name: "IX_ReplayPlayerNgRatings_ReplayPlayerId",
                table: "ReplayPlayerNgRatings",
                column: "ReplayPlayerId");

            migrationBuilder.AddForeignKey(
                name: "FK_ArcadeReplays_Replays_ReplayId",
                table: "ArcadeReplays",
                column: "ReplayId",
                principalTable: "Replays",
                principalColumn: "ReplayId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArcadeReplays_Replays_ReplayId",
                table: "ArcadeReplays");

            migrationBuilder.DropTable(
                name: "PlayerNgRatings");

            migrationBuilder.DropTable(
                name: "ReplayNgRatings");

            migrationBuilder.DropTable(
                name: "ReplayPlayerNgRatings");

            migrationBuilder.DropIndex(
                name: "IX_ArcadeReplays_ReplayId",
                table: "ArcadeReplays");

            migrationBuilder.DropColumn(
                name: "ReplayId",
                table: "ArcadeReplays");

            migrationBuilder.AddColumn<string>(
                name: "ReplayHash",
                table: "ArcadeReplays",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ArcadeReplays_ReplayHash",
                table: "ArcadeReplays",
                column: "ReplayHash");
        }
    }
}
