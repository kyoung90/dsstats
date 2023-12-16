using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mysql8Migrations.Migrations
{
    /// <inheritdoc />
    public partial class StoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var SetPlayerRatingPos = @"CREATE PROCEDURE `SetPlayerRatingPos`()
BEGIN
    UPDATE PlayerRatings AS pr
    INNER JOIN (
        SELECT
            PlayerRatingId,
            RANK() OVER (PARTITION BY RatingType ORDER BY Rating DESC, PlayerId) AS Pos
        FROM
            PlayerRatings
    ) AS ranked
    ON pr.PlayerRatingId = ranked.PlayerRatingId
    SET pr.Pos = ranked.Pos;
END
";
            var SetComboPlayerRatingPos = @"CREATE PROCEDURE `SetComboPlayerRatingPos`()
BEGIN
    UPDATE ComboPlayerRatings AS cpr
    INNER JOIN (
        SELECT
            PlayerId,
            RANK() OVER (PARTITION BY RatingType ORDER BY Rating DESC, PlayerId) AS Pos
        FROM
            ComboPlayerRatings
    ) AS ranked
    ON cpr.PlayerId = ranked.PlayerId
    SET cpr.Pos = ranked.Pos;
END
";
            var SetArcadePlayerRatingPos = @"CREATE PROCEDURE `SetArcadePlayerRatingPos`()
BEGIN
    UPDATE ArcadePlayerRatings AS apr
    INNER JOIN (
        SELECT
            ArcadePlayerId,
            RANK() OVER (PARTITION BY RatingType ORDER BY Rating DESC, ArcadePlayerId) AS Pos
        FROM
            ArcadePlayerRatings
    ) AS ranked
    ON apr.ArcadePlayerId = ranked.ArcadePlayerId
    SET apr.Pos = ranked.Pos;
END
";
            var CreateMaterializedArcadeReplays = @"CREATE PROCEDURE `CreateMaterializedArcadeReplays`()
BEGIN
	TRUNCATE TABLE MaterializedArcadeReplays;
    INSERT INTO MaterializedArcadeReplays (ArcadeReplayId, CreatedAt, WinnerTeam, Duration, GameMode)
    SELECT `a`.`ArcadeReplayId`, `a`.`CreatedAt`, `a`.`WinnerTeam`, `a`.`Duration`, `a`.`GameMode`
    FROM `ArcadeReplays` AS `a`
    WHERE (((((`a`.`CreatedAt` >= '2021-02-01') AND (`a`.`PlayerCount` = 6)) AND (`a`.`Duration` >= 300)) AND (`a`.`WinnerTeam` > 0)) AND NOT (`a`.`TournamentEdition`)) AND `a`.`GameMode` IN (3, 7, 4)
    ORDER BY `a`.`CreatedAt`, `a`.`ArcadeReplayId`;
END
";

            migrationBuilder.Sql(SetArcadePlayerRatingPos);
            migrationBuilder.Sql(SetComboPlayerRatingPos);
            migrationBuilder.Sql(SetPlayerRatingPos);
            migrationBuilder.Sql(CreateMaterializedArcadeReplays);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
