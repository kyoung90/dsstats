using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MysqlMigrations.Migrations
{
    /// <inheritdoc />
    public partial class AracdeReplayOnDeleteNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArcadeReplays_Replays_ReplayId",
                table: "ArcadeReplays");

            migrationBuilder.AddForeignKey(
                name: "FK_ArcadeReplays_Replays_ReplayId",
                table: "ArcadeReplays",
                column: "ReplayId",
                principalTable: "Replays",
                principalColumn: "ReplayId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ArcadeReplays_Replays_ReplayId",
                table: "ArcadeReplays");

            migrationBuilder.AddForeignKey(
                name: "FK_ArcadeReplays_Replays_ReplayId",
                table: "ArcadeReplays",
                column: "ReplayId",
                principalTable: "Replays",
                principalColumn: "ReplayId");
        }
    }
}
