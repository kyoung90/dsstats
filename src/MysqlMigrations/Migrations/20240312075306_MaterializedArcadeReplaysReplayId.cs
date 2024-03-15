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

            var sql2 = @"DROP PROCEDURE IF EXISTS SetPlayerRatingNgPos;
CREATE PROCEDURE `SetPlayerRatingNgPos`()
BEGIN
    DECLARE done INT DEFAULT FALSE;
    DECLARE rating_type INT;
    DECLARE cur CURSOR FOR SELECT DISTINCT RatingNgType FROM PlayerNgRatings;
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;

    OPEN cur;

    read_loop: LOOP
        FETCH cur INTO rating_type;
        IF done THEN
            LEAVE read_loop;
        END IF;

        SET @pos = 0;
        UPDATE PlayerNgRatings
        SET Pos = (@pos:=@pos+1)
        WHERE RatingNgType = rating_type
        ORDER BY Rating DESC, PlayerId;
    END LOOP;

    CLOSE cur;
END
";
            migrationBuilder.Sql(sql);
            migrationBuilder.Sql(sql2);
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
