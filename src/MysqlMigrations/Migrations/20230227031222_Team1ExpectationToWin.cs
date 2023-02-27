using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MysqlMigrations.Migrations
{
    public partial class Team1ExpectationToWin : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Team1ExpectationToWin",
                table: "ReplayRatings",
                type: "double",
                nullable: false,
                defaultValue: 0.0);
            
            var sp = @"ALTER TABLE ReplayRatings MODIFY ReplayId int(11) AFTER Team1ExpectationToWin;";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Team1ExpectationToWin",
                table: "ReplayRatings");
        }
    }
}
