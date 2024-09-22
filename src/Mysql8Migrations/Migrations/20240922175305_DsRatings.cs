using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mysql8Migrations.Migrations
{
    /// <inheritdoc />
    public partial class DsRatings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerDsRatings",
                columns: table => new
                {
                    PlayerDsRatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RatingType = table.Column<int>(type: "int", nullable: false),
                    Games = table.Column<int>(type: "int", nullable: false),
                    Wins = table.Column<int>(type: "int", nullable: false),
                    Mvps = table.Column<int>(type: "int", nullable: false),
                    Mmr = table.Column<double>(type: "double", nullable: false),
                    Consistency = table.Column<double>(type: "double", nullable: false),
                    Confidence = table.Column<double>(type: "double", nullable: false),
                    PeakRating = table.Column<float>(type: "FLOAT(8, 2)", nullable: false),
                    RecentRatingGain = table.Column<float>(type: "FLOAT(8, 2)", nullable: false),
                    MainCmdr = table.Column<int>(type: "int", nullable: false),
                    MainPercentage = table.Column<float>(type: "FLOAT(8, 2)", nullable: false),
                    WinStreak = table.Column<int>(type: "int", nullable: false),
                    LoseStreak = table.Column<int>(type: "int", nullable: false),
                    CurrentStreak = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    LatestReplay = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerDsRatings", x => x.PlayerDsRatingId);
                    table.ForeignKey(
                        name: "FK_PlayerDsRatings_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ReplayPlayerDsRatings",
                columns: table => new
                {
                    ReplayPlayerDsRatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Rating = table.Column<float>(type: "FLOAT(8, 2)", nullable: false),
                    RatingChange = table.Column<float>(type: "FLOAT(8, 2)", nullable: false),
                    Games = table.Column<int>(type: "int", nullable: false),
                    CmdrGames = table.Column<int>(type: "int", nullable: false),
                    Consistency = table.Column<float>(type: "FLOAT(8, 2)", nullable: false),
                    Confidence = table.Column<float>(type: "FLOAT(8, 2)", nullable: false),
                    ReplayPlayerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplayPlayerDsRatings", x => x.ReplayPlayerDsRatingId);
                    table.ForeignKey(
                        name: "FK_ReplayPlayerDsRatings_ReplayPlayers_ReplayPlayerId",
                        column: x => x.ReplayPlayerId,
                        principalTable: "ReplayPlayers",
                        principalColumn: "ReplayPlayerId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerDsRatings_PlayerId_RatingType",
                table: "PlayerDsRatings",
                columns: new[] { "PlayerId", "RatingType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReplayPlayerDsRatings_ReplayPlayerId",
                table: "ReplayPlayerDsRatings",
                column: "ReplayPlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerDsRatings");

            migrationBuilder.DropTable(
                name: "ReplayPlayerDsRatings");
        }
    }
}
