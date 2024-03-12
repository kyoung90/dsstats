using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MysqlMigrations.Migrations
{
    /// <inheritdoc />
    public partial class MaterializedArcadeReplaysReplayId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReplayId",
                table: "MaterializedArcadeReplays",
                type: "int",
                nullable: true);

            var sql = @"DROP PROCEDURE IF EXISTS CreateMaterializedArcadeReplays;
CREATE PROCEDURE `CreateMaterializedArcadeReplays`()
BEGIN
	TRUNCATE TABLE MaterializedArcadeReplays;
    INSERT INTO MaterializedArcadeReplays (ArcadeReplayId, CreatedAt, WinnerTeam, Duration, GameMode, ReplayId)
    SELECT `a`.`ArcadeReplayId`, `a`.`CreatedAt`, `a`.`WinnerTeam`, `a`.`Duration`, `a`.`GameMode`, `a`.`ReplayId`
    FROM `ArcadeReplays` AS `a`
    WHERE (((((`a`.`CreatedAt` >= '2021-02-01') AND (`a`.`PlayerCount` = 6)) AND (`a`.`Duration` >= 300)) AND (`a`.`WinnerTeam` > 0)) AND NOT (`a`.`TournamentEdition`)) AND `a`.`GameMode` IN (3, 7, 4)
    ORDER BY `a`.`CreatedAt`, `a`.`ArcadeReplayId`;
END
";
            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReplayId",
                table: "MaterializedArcadeReplays");

            var sql = @"DROP PROCEDURE IF EXISTS CreateMaterializedArcadeReplays;
CREATE PROCEDURE `CreateMaterializedArcadeReplays`()
BEGIN
	TRUNCATE TABLE MaterializedArcadeReplays;
    INSERT INTO MaterializedArcadeReplays (ArcadeReplayId, CreatedAt, WinnerTeam, Duration, GameMode)
    SELECT `a`.`ArcadeReplayId`, `a`.`CreatedAt`, `a`.`WinnerTeam`, `a`.`Duration`, `a`.`GameMode`
    FROM `ArcadeReplays` AS `a`
    WHERE (((((`a`.`CreatedAt` >= '2021-02-01') AND (`a`.`PlayerCount` = 6)) AND (`a`.`Duration` >= 300)) AND (`a`.`WinnerTeam` > 0)) AND NOT (`a`.`TournamentEdition`)) AND `a`.`GameMode` IN (3, 7, 4)
    ORDER BY `a`.`CreatedAt`, `a`.`ArcadeReplayId`;
END
";
            migrationBuilder.Sql(sql);
        }
    }
}
