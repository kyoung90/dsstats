using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MysqlMigrations.Migrations
{
    /// <inheritdoc />
    public partial class Tourney : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TourneyMatchId",
                table: "Replays",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Tourneys",
                columns: table => new
                {
                    TourneyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TourneyGuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    WinnerTeamId = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    GameMode = table.Column<int>(type: "int", nullable: false),
                    WinnerTeam = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tourneys", x => x.TourneyId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TourneyMatches",
                columns: table => new
                {
                    TourneyMatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TourneyMatchGuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Round = table.Column<int>(type: "int", nullable: false),
                    Group = table.Column<int>(type: "int", nullable: false),
                    IsLowerBracket = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TourneyId = table.Column<int>(type: "int", nullable: false),
                    Ban1 = table.Column<int>(type: "int", nullable: false),
                    Ban2 = table.Column<int>(type: "int", nullable: false),
                    Ban3 = table.Column<int>(type: "int", nullable: false),
                    MatchResult = table.Column<int>(type: "int", nullable: false),
                    TeamAGuid = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourneyMatches", x => x.TourneyMatchId);
                    table.ForeignKey(
                        name: "FK_TourneyMatches_Tourneys_TourneyId",
                        column: x => x.TourneyId,
                        principalTable: "Tourneys",
                        principalColumn: "TourneyId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TourneyTeams",
                columns: table => new
                {
                    TourneyTeamId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TeamGuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TourneyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourneyTeams", x => x.TourneyTeamId);
                    table.ForeignKey(
                        name: "FK_TourneyTeams_Tourneys_TourneyId",
                        column: x => x.TourneyId,
                        principalTable: "Tourneys",
                        principalColumn: "TourneyId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TourneyMatchTourneyTeam",
                columns: table => new
                {
                    TourneyMatchesTourneyMatchId = table.Column<int>(type: "int", nullable: false),
                    TourneyTeamsTourneyTeamId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourneyMatchTourneyTeam", x => new { x.TourneyMatchesTourneyMatchId, x.TourneyTeamsTourneyTeamId });
                    table.ForeignKey(
                        name: "FK_TourneyMatchTourneyTeam_TourneyMatches_TourneyMatchesTourney~",
                        column: x => x.TourneyMatchesTourneyMatchId,
                        principalTable: "TourneyMatches",
                        principalColumn: "TourneyMatchId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TourneyMatchTourneyTeam_TourneyTeams_TourneyTeamsTourneyTeam~",
                        column: x => x.TourneyTeamsTourneyTeamId,
                        principalTable: "TourneyTeams",
                        principalColumn: "TourneyTeamId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TourneyPlayers",
                columns: table => new
                {
                    TourneyPlayerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TourneyPlayerGuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    TourneyId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    TourneyTeamId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TourneyPlayers", x => x.TourneyPlayerId);
                    table.ForeignKey(
                        name: "FK_TourneyPlayers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TourneyPlayers_TourneyTeams_TourneyTeamId",
                        column: x => x.TourneyTeamId,
                        principalTable: "TourneyTeams",
                        principalColumn: "TourneyTeamId");
                    table.ForeignKey(
                        name: "FK_TourneyPlayers_Tourneys_TourneyId",
                        column: x => x.TourneyId,
                        principalTable: "Tourneys",
                        principalColumn: "TourneyId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Replays_TourneyMatchId",
                table: "Replays",
                column: "TourneyMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_TourneyMatchTourneyTeam_TourneyTeamsTourneyTeamId",
                table: "TourneyMatchTourneyTeam",
                column: "TourneyTeamsTourneyTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TourneyMatches_TourneyId",
                table: "TourneyMatches",
                column: "TourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_TourneyMatches_TourneyMatchGuid",
                table: "TourneyMatches",
                column: "TourneyMatchGuid");

            migrationBuilder.CreateIndex(
                name: "IX_TourneyPlayers_PlayerId",
                table: "TourneyPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TourneyPlayers_TourneyId",
                table: "TourneyPlayers",
                column: "TourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_TourneyPlayers_TourneyPlayerGuid",
                table: "TourneyPlayers",
                column: "TourneyPlayerGuid");

            migrationBuilder.CreateIndex(
                name: "IX_TourneyPlayers_TourneyTeamId",
                table: "TourneyPlayers",
                column: "TourneyTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_TourneyTeams_TeamGuid",
                table: "TourneyTeams",
                column: "TeamGuid");

            migrationBuilder.CreateIndex(
                name: "IX_TourneyTeams_TourneyId",
                table: "TourneyTeams",
                column: "TourneyId");

            migrationBuilder.CreateIndex(
                name: "IX_Tourneys_TourneyGuid",
                table: "Tourneys",
                column: "TourneyGuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Replays_TourneyMatches_TourneyMatchId",
                table: "Replays",
                column: "TourneyMatchId",
                principalTable: "TourneyMatches",
                principalColumn: "TourneyMatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Replays_TourneyMatches_TourneyMatchId",
                table: "Replays");

            migrationBuilder.DropTable(
                name: "TourneyMatchTourneyTeam");

            migrationBuilder.DropTable(
                name: "TourneyPlayers");

            migrationBuilder.DropTable(
                name: "TourneyMatches");

            migrationBuilder.DropTable(
                name: "TourneyTeams");

            migrationBuilder.DropTable(
                name: "Tourneys");

            migrationBuilder.DropIndex(
                name: "IX_Replays_TourneyMatchId",
                table: "Replays");

            migrationBuilder.DropColumn(
                name: "TourneyMatchId",
                table: "Replays");
        }
    }
}
