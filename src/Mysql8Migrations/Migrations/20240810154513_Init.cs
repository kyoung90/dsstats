using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mysql8Migrations.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArcadeReplays",
                columns: table => new
                {
                    ArcadeReplayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    BnetBucketId = table.Column<long>(type: "bigint", nullable: false),
                    BnetRecordId = table.Column<long>(type: "bigint", nullable: false),
                    GameMode = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    PlayerCount = table.Column<int>(type: "int", nullable: false),
                    TournamentEdition = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    WinnerTeam = table.Column<int>(type: "int", nullable: false),
                    Imported = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    ReplayHash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArcadeReplays", x => x.ArcadeReplayId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CommanderMmrs",
                columns: table => new
                {
                    CommanderMmrId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Race = table.Column<int>(type: "int", nullable: false),
                    OppRace = table.Column<int>(type: "int", nullable: false),
                    SynergyMmr = table.Column<double>(type: "double", nullable: false),
                    AntiSynergyMmr = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommanderMmrs", x => x.CommanderMmrId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DsAbilities",
                columns: table => new
                {
                    DsAbilityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Commander = table.Column<int>(type: "int", nullable: false),
                    Requirements = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cooldown = table.Column<int>(type: "int", nullable: false),
                    GlobalTimer = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    EnergyCost = table.Column<float>(type: "float", nullable: false),
                    CastRange = table.Column<int>(type: "int", nullable: false),
                    AoeRadius = table.Column<float>(type: "float", nullable: false),
                    AbilityTarget = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(310)", maxLength: 310, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DsAbilities", x => x.DsAbilityId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DsPickBans",
                columns: table => new
                {
                    DsPickBanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PickBanMode = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    Bans = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Picks = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DsPickBans", x => x.DsPickBanId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DsUnits",
                columns: table => new
                {
                    DsUnitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Commander = table.Column<int>(type: "int", nullable: false),
                    Tier = table.Column<int>(type: "int", nullable: false),
                    Cost = table.Column<int>(type: "int", nullable: false),
                    Life = table.Column<int>(type: "int", nullable: false),
                    Shields = table.Column<int>(type: "int", nullable: false),
                    Speed = table.Column<float>(type: "float", nullable: false),
                    Armor = table.Column<int>(type: "int", nullable: false),
                    ShieldArmor = table.Column<int>(type: "int", nullable: false),
                    StartingEnergy = table.Column<int>(type: "int", nullable: false),
                    MaxEnergy = table.Column<int>(type: "int", nullable: false),
                    HealthRegen = table.Column<float>(type: "float", nullable: false),
                    EnergyRegen = table.Column<float>(type: "float", nullable: false),
                    UnitType = table.Column<int>(type: "int", nullable: false),
                    MovementType = table.Column<int>(type: "int", nullable: false),
                    Size = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<int>(type: "int", nullable: false),
                    UnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DsUnits", x => x.DsUnitId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DsUpdates",
                columns: table => new
                {
                    DsUpdateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Commander = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    DiscordId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Change = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DsUpdates", x => x.DsUpdateId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    EventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EventGuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    EventStart = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    WinnerTeam = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GameMode = table.Column<int>(type: "int", nullable: false),
                    ExternalLink = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.EventId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Faqs",
                columns: table => new
                {
                    FaqId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Question = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Answer = table.Column<string>(type: "varchar(400)", maxLength: 400, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Upvotes = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faqs", x => x.FaqId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FaqVotes",
                columns: table => new
                {
                    FaqVoteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FaqId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaqVotes", x => x.FaqVoteId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FunStatMemories",
                columns: table => new
                {
                    FunStatsMemoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Created = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    RatingType = table.Column<int>(type: "int", nullable: false),
                    TimePeriod = table.Column<int>(type: "int", nullable: false),
                    TotalTimePlayed = table.Column<long>(type: "bigint", nullable: false),
                    AvgGameDuration = table.Column<int>(type: "int", nullable: false),
                    UnitNameMost = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitCountMost = table.Column<int>(type: "int", nullable: false),
                    UnitNameLeast = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitCountLeast = table.Column<int>(type: "int", nullable: false),
                    FirstReplay = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GreatestArmyReplay = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MostUpgradesReplay = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MostCompetitiveReplay = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GreatestComebackReplay = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunStatMemories", x => x.FunStatsMemoryId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "IhSessions",
                columns: table => new
                {
                    IhSessionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RatingType = table.Column<int>(type: "int", nullable: false),
                    GroupId = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    Created = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    Players = table.Column<int>(type: "int", nullable: false),
                    Games = table.Column<int>(type: "int", nullable: false),
                    Closed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    GroupState = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GroupStateV2 = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IhSessions", x => x.IhSessionId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "MaterializedArcadeReplays",
                columns: table => new
                {
                    MaterializedArcadeReplayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ArcadeReplayId = table.Column<int>(type: "int", nullable: false),
                    GameMode = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    WinnerTeam = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterializedArcadeReplays", x => x.MaterializedArcadeReplayId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ReplayArcadeMatches",
                columns: table => new
                {
                    ReplayArcadeMatchId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReplayId = table.Column<int>(type: "int", nullable: false),
                    ArcadeReplayId = table.Column<int>(type: "int", nullable: false),
                    MatchTime = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplayArcadeMatches", x => x.ReplayArcadeMatchId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ReplayDownloadCounts",
                columns: table => new
                {
                    ReplayDownloadCountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReplayHash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplayDownloadCounts", x => x.ReplayDownloadCountId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ReplayViewCounts",
                columns: table => new
                {
                    ReplayViewCountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ReplayHash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplayViewCounts", x => x.ReplayViewCountId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SkipReplays",
                columns: table => new
                {
                    SkipReplayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Path = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SkipReplays", x => x.SkipReplayId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "StreakInfos",
                columns: table => new
                {
                    PlayerResult = table.Column<int>(type: "int", nullable: false),
                    LongestStreak = table.Column<double>(type: "double", nullable: false)
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Units",
                columns: table => new
                {
                    UnitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Units", x => x.UnitId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Upgrades",
                columns: table => new
                {
                    UpgradeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Cost = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Upgrades", x => x.UpgradeId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Uploaders",
                columns: table => new
                {
                    UploaderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AppGuid = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    AppVersion = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Identifier = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LatestUpload = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    LatestReplay = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    Games = table.Column<int>(type: "int", nullable: false),
                    Wins = table.Column<int>(type: "int", nullable: false),
                    Mvp = table.Column<int>(type: "int", nullable: false),
                    MainCommander = table.Column<int>(type: "int", nullable: false),
                    MainCount = table.Column<int>(type: "int", nullable: false),
                    TeamGames = table.Column<int>(type: "int", nullable: false),
                    UploadLastDisabled = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    UploadDisabledCount = table.Column<int>(type: "int", nullable: false),
                    UploadIsDisabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Uploaders", x => x.UploaderId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArcadeReplayRatings",
                columns: table => new
                {
                    ArcadeReplayRatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RatingType = table.Column<int>(type: "int", nullable: false),
                    LeaverType = table.Column<int>(type: "int", nullable: false),
                    ExpectationToWin = table.Column<float>(type: "float", nullable: false),
                    ArcadeReplayId = table.Column<int>(type: "int", nullable: false),
                    AvgRating = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArcadeReplayRatings", x => x.ArcadeReplayRatingId);
                    table.ForeignKey(
                        name: "FK_ArcadeReplayRatings_ArcadeReplays_ArcadeReplayId",
                        column: x => x.ArcadeReplayId,
                        principalTable: "ArcadeReplays",
                        principalColumn: "ArcadeReplayId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DsAbilityDsUnit",
                columns: table => new
                {
                    AbilitiesDsAbilityId = table.Column<int>(type: "int", nullable: false),
                    DsUnitsDsUnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DsAbilityDsUnit", x => new { x.AbilitiesDsAbilityId, x.DsUnitsDsUnitId });
                    table.ForeignKey(
                        name: "FK_DsAbilityDsUnit_DsAbilities_AbilitiesDsAbilityId",
                        column: x => x.AbilitiesDsAbilityId,
                        principalTable: "DsAbilities",
                        principalColumn: "DsAbilityId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DsAbilityDsUnit_DsUnits_DsUnitsDsUnitId",
                        column: x => x.DsUnitsDsUnitId,
                        principalTable: "DsUnits",
                        principalColumn: "DsUnitId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DsUpgrades",
                columns: table => new
                {
                    DsUpgradeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Upgrade = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Commander = table.Column<int>(type: "int", nullable: false),
                    Cost = table.Column<int>(type: "int", nullable: false),
                    RequiredTier = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DsUnitId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DsUpgrades", x => x.DsUpgradeId);
                    table.ForeignKey(
                        name: "FK_DsUpgrades_DsUnits_DsUnitId",
                        column: x => x.DsUnitId,
                        principalTable: "DsUnits",
                        principalColumn: "DsUnitId");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DsWeapons",
                columns: table => new
                {
                    DsWeaponId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Range = table.Column<float>(type: "float", nullable: false),
                    AttackSpeed = table.Column<float>(type: "float", nullable: false),
                    Attacks = table.Column<int>(type: "int", nullable: false),
                    CanTarget = table.Column<int>(type: "int", nullable: false),
                    Damage = table.Column<int>(type: "int", nullable: false),
                    DamagePerUpgrade = table.Column<int>(type: "int", nullable: false),
                    DsUnitId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DsWeapons", x => x.DsWeaponId);
                    table.ForeignKey(
                        name: "FK_DsWeapons_DsUnits_DsUnitId",
                        column: x => x.DsUnitId,
                        principalTable: "DsUnits",
                        principalColumn: "DsUnitId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ReplayEvents",
                columns: table => new
                {
                    ReplayEventId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Round = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    WinnerTeam = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RunnerTeam = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ban1 = table.Column<int>(type: "int", nullable: false),
                    Ban2 = table.Column<int>(type: "int", nullable: false),
                    Ban3 = table.Column<int>(type: "int", nullable: false),
                    Ban4 = table.Column<int>(type: "int", nullable: false),
                    Ban5 = table.Column<int>(type: "int", nullable: false),
                    EventId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplayEvents", x => x.ReplayEventId);
                    table.ForeignKey(
                        name: "FK_ReplayEvents_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "EventId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BattleNetInfos",
                columns: table => new
                {
                    BattleNetInfoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BattleNetId = table.Column<int>(type: "int", nullable: false),
                    UploaderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BattleNetInfos", x => x.BattleNetInfoId);
                    table.ForeignKey(
                        name: "FK_BattleNetInfos_Uploaders_UploaderId",
                        column: x => x.UploaderId,
                        principalTable: "Uploaders",
                        principalColumn: "UploaderId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    PlayerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ToonId = table.Column<int>(type: "int", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    RealmId = table.Column<int>(type: "int", nullable: false),
                    NotUploadCount = table.Column<int>(type: "int", nullable: false),
                    DisconnectCount = table.Column<int>(type: "int", nullable: false),
                    RageQuitCount = table.Column<int>(type: "int", nullable: false),
                    ArcadeDefeatsSinceLastUpload = table.Column<int>(type: "int", nullable: false),
                    UploaderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.PlayerId);
                    table.ForeignKey(
                        name: "FK_Players_Uploaders_UploaderId",
                        column: x => x.UploaderId,
                        principalTable: "Uploaders",
                        principalColumn: "UploaderId");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BonusDamages",
                columns: table => new
                {
                    BonusDamageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UnitType = table.Column<int>(type: "int", nullable: false),
                    Damage = table.Column<int>(type: "int", nullable: false),
                    PerUpgrade = table.Column<int>(type: "int", nullable: false),
                    DsWeaponId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonusDamages", x => x.BonusDamageId);
                    table.ForeignKey(
                        name: "FK_BonusDamages_DsWeapons_DsWeaponId",
                        column: x => x.DsWeaponId,
                        principalTable: "DsWeapons",
                        principalColumn: "DsWeaponId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Replays",
                columns: table => new
                {
                    ReplayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FileName = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Uploaded = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TournamentEdition = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    GameTime = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    Imported = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: true),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    WinnerTeam = table.Column<int>(type: "int", nullable: false),
                    PlayerResult = table.Column<int>(type: "int", nullable: false),
                    PlayerPos = table.Column<int>(type: "int", nullable: false),
                    ResultCorrected = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    GameMode = table.Column<int>(type: "int", nullable: false),
                    Objective = table.Column<int>(type: "int", nullable: false),
                    Bunker = table.Column<int>(type: "int", nullable: false),
                    Cannon = table.Column<int>(type: "int", nullable: false),
                    Minkillsum = table.Column<int>(type: "int", nullable: false),
                    Maxkillsum = table.Column<int>(type: "int", nullable: false),
                    Minarmy = table.Column<int>(type: "int", nullable: false),
                    Minincome = table.Column<int>(type: "int", nullable: false),
                    Maxleaver = table.Column<int>(type: "int", nullable: false),
                    Playercount = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    ReplayHash = table.Column<string>(type: "char(64)", fixedLength: true, maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DefaultFilter = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Views = table.Column<int>(type: "int", nullable: false),
                    Downloads = table.Column<int>(type: "int", nullable: false),
                    Middle = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CommandersTeam1 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CommandersTeam2 = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ReplayEventId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Replays", x => x.ReplayId);
                    table.ForeignKey(
                        name: "FK_Replays_ReplayEvents_ReplayEventId",
                        column: x => x.ReplayEventId,
                        principalTable: "ReplayEvents",
                        principalColumn: "ReplayEventId");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArcadePlayerRatings",
                columns: table => new
                {
                    ArcadePlayerRatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RatingType = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<double>(type: "double", nullable: false),
                    Pos = table.Column<int>(type: "int", nullable: false),
                    Games = table.Column<int>(type: "int", nullable: false),
                    Wins = table.Column<int>(type: "int", nullable: false),
                    Mvp = table.Column<int>(type: "int", nullable: false),
                    TeamGames = table.Column<int>(type: "int", nullable: false),
                    MainCount = table.Column<int>(type: "int", nullable: false),
                    Main = table.Column<int>(type: "int", nullable: false),
                    Consistency = table.Column<double>(type: "double", nullable: false),
                    Confidence = table.Column<double>(type: "double", nullable: false),
                    IsUploader = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArcadePlayerRatings", x => x.ArcadePlayerRatingId);
                    table.ForeignKey(
                        name: "FK_ArcadePlayerRatings_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArcadeReplayDsPlayers",
                columns: table => new
                {
                    ArcadeReplayDsPlayerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SlotNumber = table.Column<int>(type: "int", nullable: false),
                    Team = table.Column<int>(type: "int", nullable: false),
                    Discriminator = table.Column<int>(type: "int", nullable: false),
                    PlayerResult = table.Column<int>(type: "int", nullable: false),
                    ArcadeReplayId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArcadeReplayDsPlayers", x => x.ArcadeReplayDsPlayerId);
                    table.ForeignKey(
                        name: "FK_ArcadeReplayDsPlayers_ArcadeReplays_ArcadeReplayId",
                        column: x => x.ArcadeReplayId,
                        principalTable: "ArcadeReplays",
                        principalColumn: "ArcadeReplayId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArcadeReplayDsPlayers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ComboPlayerRatings",
                columns: table => new
                {
                    ComboPlayerRatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RatingType = table.Column<int>(type: "int", nullable: false),
                    Games = table.Column<int>(type: "int", nullable: false),
                    Wins = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<double>(type: "double", nullable: false),
                    Consistency = table.Column<double>(type: "double", nullable: false),
                    Confidence = table.Column<double>(type: "double", nullable: false),
                    Pos = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboPlayerRatings", x => x.ComboPlayerRatingId);
                    table.ForeignKey(
                        name: "FK_ComboPlayerRatings_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "IhSessionPlayers",
                columns: table => new
                {
                    IhSessionPlayerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Games = table.Column<int>(type: "int", nullable: false),
                    Wins = table.Column<int>(type: "int", nullable: false),
                    Obs = table.Column<int>(type: "int", nullable: false),
                    RatingStart = table.Column<int>(type: "int", nullable: false),
                    RatingEnd = table.Column<int>(type: "int", nullable: false),
                    Performance = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    IhSessionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IhSessionPlayers", x => x.IhSessionPlayerId);
                    table.ForeignKey(
                        name: "FK_IhSessionPlayers_IhSessions_IhSessionId",
                        column: x => x.IhSessionId,
                        principalTable: "IhSessions",
                        principalColumn: "IhSessionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IhSessionPlayers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "NoUploadResults",
                columns: table => new
                {
                    NoUploadResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TotalReplays = table.Column<int>(type: "int", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    LatestReplay = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    NoUploadTotal = table.Column<int>(type: "int", nullable: false),
                    NoUploadDefeats = table.Column<int>(type: "int", nullable: false),
                    LatestNoUpload = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    LatestUpload = table.Column<DateTime>(type: "datetime(0)", precision: 0, nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoUploadResults", x => x.NoUploadResultId);
                    table.ForeignKey(
                        name: "FK_NoUploadResults_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PlayerRatings",
                columns: table => new
                {
                    PlayerRatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RatingType = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<double>(type: "double", nullable: false),
                    Games = table.Column<int>(type: "int", nullable: false),
                    Wins = table.Column<int>(type: "int", nullable: false),
                    Mvp = table.Column<int>(type: "int", nullable: false),
                    TeamGames = table.Column<int>(type: "int", nullable: false),
                    MainCount = table.Column<int>(type: "int", nullable: false),
                    Main = table.Column<int>(type: "int", nullable: false),
                    Consistency = table.Column<double>(type: "double", nullable: false),
                    Confidence = table.Column<double>(type: "double", nullable: false),
                    IsUploader = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Pos = table.Column<int>(type: "int", nullable: false),
                    ArcadeDefeatsSinceLastUpload = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerRatings", x => x.PlayerRatingId);
                    table.ForeignKey(
                        name: "FK_PlayerRatings_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ComboReplayRatings",
                columns: table => new
                {
                    ComboReplayRatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RatingType = table.Column<int>(type: "int", nullable: false),
                    LeaverType = table.Column<int>(type: "int", nullable: false),
                    ExpectationToWin = table.Column<double>(type: "double", precision: 5, scale: 2, nullable: false),
                    ReplayId = table.Column<int>(type: "int", nullable: false),
                    IsPreRating = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AvgRating = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboReplayRatings", x => x.ComboReplayRatingId);
                    table.ForeignKey(
                        name: "FK_ComboReplayRatings_Replays_ReplayId",
                        column: x => x.ReplayId,
                        principalTable: "Replays",
                        principalColumn: "ReplayId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ReplayPlayers",
                columns: table => new
                {
                    ReplayPlayerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Clan = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    GamePos = table.Column<int>(type: "int", nullable: false),
                    Team = table.Column<int>(type: "int", nullable: false),
                    PlayerResult = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    Race = table.Column<int>(type: "int", nullable: false),
                    OppRace = table.Column<int>(type: "int", nullable: false),
                    APM = table.Column<int>(type: "int", nullable: false),
                    Income = table.Column<int>(type: "int", nullable: false),
                    Army = table.Column<int>(type: "int", nullable: false),
                    Kills = table.Column<int>(type: "int", nullable: false),
                    UpgradesSpent = table.Column<int>(type: "int", nullable: false),
                    IsUploader = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    IsLeaver = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DidNotUpload = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    TierUpgrades = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Refineries = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LastSpawnHash = table.Column<string>(type: "char(64)", fixedLength: true, maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Downloads = table.Column<int>(type: "int", nullable: false),
                    Views = table.Column<int>(type: "int", nullable: false),
                    ReplayId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    UpgradeId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplayPlayers", x => x.ReplayPlayerId);
                    table.ForeignKey(
                        name: "FK_ReplayPlayers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "PlayerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReplayPlayers_Replays_ReplayId",
                        column: x => x.ReplayId,
                        principalTable: "Replays",
                        principalColumn: "ReplayId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReplayPlayers_Upgrades_UpgradeId",
                        column: x => x.UpgradeId,
                        principalTable: "Upgrades",
                        principalColumn: "UpgradeId");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ReplayRatings",
                columns: table => new
                {
                    ReplayRatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RatingType = table.Column<int>(type: "int", nullable: false),
                    LeaverType = table.Column<int>(type: "int", nullable: false),
                    ExpectationToWin = table.Column<float>(type: "float", nullable: false),
                    ReplayId = table.Column<int>(type: "int", nullable: false),
                    IsPreRating = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AvgRating = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReplayRatings", x => x.ReplayRatingId);
                    table.ForeignKey(
                        name: "FK_ReplayRatings_Replays_ReplayId",
                        column: x => x.ReplayId,
                        principalTable: "Replays",
                        principalColumn: "ReplayId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "UploaderReplays",
                columns: table => new
                {
                    ReplaysReplayId = table.Column<int>(type: "int", nullable: false),
                    UploadersUploaderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UploaderReplays", x => new { x.ReplaysReplayId, x.UploadersUploaderId });
                    table.ForeignKey(
                        name: "FK_UploaderReplays_Replays_ReplaysReplayId",
                        column: x => x.ReplaysReplayId,
                        principalTable: "Replays",
                        principalColumn: "ReplayId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UploaderReplays_Uploaders_UploadersUploaderId",
                        column: x => x.UploadersUploaderId,
                        principalTable: "Uploaders",
                        principalColumn: "UploaderId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArcadePlayerRatingChanges",
                columns: table => new
                {
                    ArcadePlayerRatingChangeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Change24h = table.Column<float>(type: "float", nullable: false),
                    Change10d = table.Column<float>(type: "float", nullable: false),
                    Change30d = table.Column<float>(type: "float", nullable: false),
                    ArcadePlayerRatingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArcadePlayerRatingChanges", x => x.ArcadePlayerRatingChangeId);
                    table.ForeignKey(
                        name: "FK_ArcadePlayerRatingChanges_ArcadePlayerRatings_ArcadePlayerRa~",
                        column: x => x.ArcadePlayerRatingId,
                        principalTable: "ArcadePlayerRatings",
                        principalColumn: "ArcadePlayerRatingId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ArcadeReplayDsPlayerRatings",
                columns: table => new
                {
                    ArcadeReplayDsPlayerRatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GamePos = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<float>(type: "float", nullable: false),
                    RatingChange = table.Column<float>(type: "float", nullable: false),
                    Games = table.Column<int>(type: "int", nullable: false),
                    Consistency = table.Column<float>(type: "float", nullable: false),
                    Confidence = table.Column<float>(type: "float", nullable: false),
                    ArcadeReplayDsPlayerId = table.Column<int>(type: "int", nullable: false),
                    ArcadeReplayRatingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArcadeReplayDsPlayerRatings", x => x.ArcadeReplayDsPlayerRatingId);
                    table.ForeignKey(
                        name: "FK_ArcadeReplayDsPlayerRatings_ArcadeReplayDsPlayers_ArcadeRepl~",
                        column: x => x.ArcadeReplayDsPlayerId,
                        principalTable: "ArcadeReplayDsPlayers",
                        principalColumn: "ArcadeReplayDsPlayerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArcadeReplayDsPlayerRatings_ArcadeReplayRatings_ArcadeReplay~",
                        column: x => x.ArcadeReplayRatingId,
                        principalTable: "ArcadeReplayRatings",
                        principalColumn: "ArcadeReplayRatingId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PlayerRatingChanges",
                columns: table => new
                {
                    PlayerRatingChangeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Change24h = table.Column<float>(type: "float", nullable: false),
                    Change10d = table.Column<float>(type: "float", nullable: false),
                    Change30d = table.Column<float>(type: "float", nullable: false),
                    PlayerRatingId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerRatingChanges", x => x.PlayerRatingChangeId);
                    table.ForeignKey(
                        name: "FK_PlayerRatingChanges_PlayerRatings_PlayerRatingId",
                        column: x => x.PlayerRatingId,
                        principalTable: "PlayerRatings",
                        principalColumn: "PlayerRatingId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ComboReplayPlayerRatings",
                columns: table => new
                {
                    ComboReplayPlayerRatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GamePos = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Change = table.Column<double>(type: "double", precision: 5, scale: 2, nullable: false),
                    Games = table.Column<int>(type: "int", nullable: false),
                    Consistency = table.Column<double>(type: "double", precision: 5, scale: 2, nullable: false),
                    Confidence = table.Column<double>(type: "double", precision: 5, scale: 2, nullable: false),
                    ReplayPlayerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComboReplayPlayerRatings", x => x.ComboReplayPlayerRatingId);
                    table.ForeignKey(
                        name: "FK_ComboReplayPlayerRatings_ReplayPlayers_ReplayPlayerId",
                        column: x => x.ReplayPlayerId,
                        principalTable: "ReplayPlayers",
                        principalColumn: "ReplayPlayerId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PlayerUpgrades",
                columns: table => new
                {
                    PlayerUpgradeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Gameloop = table.Column<int>(type: "int", nullable: false),
                    UpgradeId = table.Column<int>(type: "int", nullable: false),
                    ReplayPlayerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerUpgrades", x => x.PlayerUpgradeId);
                    table.ForeignKey(
                        name: "FK_PlayerUpgrades_ReplayPlayers_ReplayPlayerId",
                        column: x => x.ReplayPlayerId,
                        principalTable: "ReplayPlayers",
                        principalColumn: "ReplayPlayerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerUpgrades_Upgrades_UpgradeId",
                        column: x => x.UpgradeId,
                        principalTable: "Upgrades",
                        principalColumn: "UpgradeId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Spawns",
                columns: table => new
                {
                    SpawnId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Gameloop = table.Column<int>(type: "int", nullable: false),
                    Breakpoint = table.Column<int>(type: "int", nullable: false),
                    Income = table.Column<int>(type: "int", nullable: false),
                    GasCount = table.Column<int>(type: "int", nullable: false),
                    ArmyValue = table.Column<int>(type: "int", nullable: false),
                    KilledValue = table.Column<int>(type: "int", nullable: false),
                    UpgradeSpent = table.Column<int>(type: "int", nullable: false),
                    ReplayPlayerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spawns", x => x.SpawnId);
                    table.ForeignKey(
                        name: "FK_Spawns_ReplayPlayers_ReplayPlayerId",
                        column: x => x.ReplayPlayerId,
                        principalTable: "ReplayPlayers",
                        principalColumn: "ReplayPlayerId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RepPlayerRatings",
                columns: table => new
                {
                    RepPlayerRatingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    GamePos = table.Column<int>(type: "int", nullable: false),
                    Rating = table.Column<float>(type: "float", nullable: false),
                    RatingChange = table.Column<float>(type: "float", nullable: false),
                    Games = table.Column<int>(type: "int", nullable: false),
                    Consistency = table.Column<float>(type: "float", nullable: false),
                    Confidence = table.Column<float>(type: "float", nullable: false),
                    ReplayPlayerId = table.Column<int>(type: "int", nullable: false),
                    ReplayRatingInfoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepPlayerRatings", x => x.RepPlayerRatingId);
                    table.ForeignKey(
                        name: "FK_RepPlayerRatings_ReplayPlayers_ReplayPlayerId",
                        column: x => x.ReplayPlayerId,
                        principalTable: "ReplayPlayers",
                        principalColumn: "ReplayPlayerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepPlayerRatings_ReplayRatings_ReplayRatingInfoId",
                        column: x => x.ReplayRatingInfoId,
                        principalTable: "ReplayRatings",
                        principalColumn: "ReplayRatingId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "SpawnUnits",
                columns: table => new
                {
                    SpawnUnitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Count = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    Poss = table.Column<string>(type: "varchar(4000)", maxLength: 4000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UnitId = table.Column<int>(type: "int", nullable: false),
                    SpawnId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpawnUnits", x => x.SpawnUnitId);
                    table.ForeignKey(
                        name: "FK_SpawnUnits_Spawns_SpawnId",
                        column: x => x.SpawnId,
                        principalTable: "Spawns",
                        principalColumn: "SpawnId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SpawnUnits_Units_UnitId",
                        column: x => x.UnitId,
                        principalTable: "Units",
                        principalColumn: "UnitId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ArcadePlayerRatingChanges_ArcadePlayerRatingId",
                table: "ArcadePlayerRatingChanges",
                column: "ArcadePlayerRatingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArcadePlayerRatings_PlayerId",
                table: "ArcadePlayerRatings",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_ArcadePlayerRatings_RatingType",
                table: "ArcadePlayerRatings",
                column: "RatingType");

            migrationBuilder.CreateIndex(
                name: "IX_ArcadeReplayDsPlayerRatings_ArcadeReplayDsPlayerId",
                table: "ArcadeReplayDsPlayerRatings",
                column: "ArcadeReplayDsPlayerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArcadeReplayDsPlayerRatings_ArcadeReplayRatingId",
                table: "ArcadeReplayDsPlayerRatings",
                column: "ArcadeReplayRatingId");

            migrationBuilder.CreateIndex(
                name: "IX_ArcadeReplayDsPlayers_ArcadeReplayId",
                table: "ArcadeReplayDsPlayers",
                column: "ArcadeReplayId");

            migrationBuilder.CreateIndex(
                name: "IX_ArcadeReplayDsPlayers_PlayerId",
                table: "ArcadeReplayDsPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_ArcadeReplayRatings_ArcadeReplayId",
                table: "ArcadeReplayRatings",
                column: "ArcadeReplayId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArcadeReplays_GameMode_CreatedAt",
                table: "ArcadeReplays",
                columns: new[] { "GameMode", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ArcadeReplays_RegionId_BnetBucketId_BnetRecordId",
                table: "ArcadeReplays",
                columns: new[] { "RegionId", "BnetBucketId", "BnetRecordId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ArcadeReplays_RegionId_GameMode_CreatedAt",
                table: "ArcadeReplays",
                columns: new[] { "RegionId", "GameMode", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ArcadeReplays_ReplayHash",
                table: "ArcadeReplays",
                column: "ReplayHash");

            migrationBuilder.CreateIndex(
                name: "IX_BattleNetInfos_UploaderId",
                table: "BattleNetInfos",
                column: "UploaderId");

            migrationBuilder.CreateIndex(
                name: "IX_BonusDamages_DsWeaponId",
                table: "BonusDamages",
                column: "DsWeaponId");

            migrationBuilder.CreateIndex(
                name: "IX_BonusDamages_UnitType",
                table: "BonusDamages",
                column: "UnitType");

            migrationBuilder.CreateIndex(
                name: "IX_ComboPlayerRatings_PlayerId",
                table: "ComboPlayerRatings",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_ComboPlayerRatings_RatingType",
                table: "ComboPlayerRatings",
                column: "RatingType");

            migrationBuilder.CreateIndex(
                name: "IX_ComboReplayPlayerRatings_ReplayPlayerId",
                table: "ComboReplayPlayerRatings",
                column: "ReplayPlayerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ComboReplayRatings_RatingType",
                table: "ComboReplayRatings",
                column: "RatingType");

            migrationBuilder.CreateIndex(
                name: "IX_ComboReplayRatings_ReplayId",
                table: "ComboReplayRatings",
                column: "ReplayId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommanderMmrs_Race_OppRace",
                table: "CommanderMmrs",
                columns: new[] { "Race", "OppRace" });

            migrationBuilder.CreateIndex(
                name: "IX_DsAbilities_Name",
                table: "DsAbilities",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_DsAbilityDsUnit_DsUnitsDsUnitId",
                table: "DsAbilityDsUnit",
                column: "DsUnitsDsUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_DsUnits_Commander",
                table: "DsUnits",
                column: "Commander");

            migrationBuilder.CreateIndex(
                name: "IX_DsUnits_Name",
                table: "DsUnits",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_DsUnits_Name_Commander",
                table: "DsUnits",
                columns: new[] { "Name", "Commander" });

            migrationBuilder.CreateIndex(
                name: "IX_DsUpdates_Time",
                table: "DsUpdates",
                column: "Time");

            migrationBuilder.CreateIndex(
                name: "IX_DsUpgrades_DsUnitId",
                table: "DsUpgrades",
                column: "DsUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_DsUpgrades_Upgrade",
                table: "DsUpgrades",
                column: "Upgrade");

            migrationBuilder.CreateIndex(
                name: "IX_DsWeapons_DsUnitId",
                table: "DsWeapons",
                column: "DsUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_Name",
                table: "Events",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Faqs_Question",
                table: "Faqs",
                column: "Question");

            migrationBuilder.CreateIndex(
                name: "IX_IhSessionPlayers_IhSessionId",
                table: "IhSessionPlayers",
                column: "IhSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_IhSessionPlayers_PlayerId",
                table: "IhSessionPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_IhSessions_GroupId",
                table: "IhSessions",
                column: "GroupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaterializedArcadeReplays_CreatedAt",
                table: "MaterializedArcadeReplays",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NoUploadResults_PlayerId",
                table: "NoUploadResults",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRatingChanges_PlayerRatingId",
                table: "PlayerRatingChanges",
                column: "PlayerRatingId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRatings_PlayerId",
                table: "PlayerRatings",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerRatings_RatingType",
                table: "PlayerRatings",
                column: "RatingType");

            migrationBuilder.CreateIndex(
                name: "IX_Players_RegionId_RealmId_ToonId",
                table: "Players",
                columns: new[] { "RegionId", "RealmId", "ToonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_UploaderId",
                table: "Players",
                column: "UploaderId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerUpgrades_ReplayPlayerId",
                table: "PlayerUpgrades",
                column: "ReplayPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerUpgrades_UpgradeId",
                table: "PlayerUpgrades",
                column: "UpgradeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReplayArcadeMatches_ArcadeReplayId",
                table: "ReplayArcadeMatches",
                column: "ArcadeReplayId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReplayArcadeMatches_MatchTime",
                table: "ReplayArcadeMatches",
                column: "MatchTime");

            migrationBuilder.CreateIndex(
                name: "IX_ReplayArcadeMatches_ReplayId",
                table: "ReplayArcadeMatches",
                column: "ReplayId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReplayEvents_EventId",
                table: "ReplayEvents",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_ReplayPlayers_IsUploader_Team",
                table: "ReplayPlayers",
                columns: new[] { "IsUploader", "Team" });

            migrationBuilder.CreateIndex(
                name: "IX_ReplayPlayers_Kills",
                table: "ReplayPlayers",
                column: "Kills");

            migrationBuilder.CreateIndex(
                name: "IX_ReplayPlayers_LastSpawnHash",
                table: "ReplayPlayers",
                column: "LastSpawnHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReplayPlayers_Name",
                table: "ReplayPlayers",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ReplayPlayers_PlayerId",
                table: "ReplayPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReplayPlayers_Race",
                table: "ReplayPlayers",
                column: "Race");

            migrationBuilder.CreateIndex(
                name: "IX_ReplayPlayers_Race_OppRace",
                table: "ReplayPlayers",
                columns: new[] { "Race", "OppRace" });

            migrationBuilder.CreateIndex(
                name: "IX_ReplayPlayers_ReplayId",
                table: "ReplayPlayers",
                column: "ReplayId");

            migrationBuilder.CreateIndex(
                name: "IX_ReplayPlayers_UpgradeId",
                table: "ReplayPlayers",
                column: "UpgradeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReplayRatings_RatingType",
                table: "ReplayRatings",
                column: "RatingType");

            migrationBuilder.CreateIndex(
                name: "IX_ReplayRatings_ReplayId",
                table: "ReplayRatings",
                column: "ReplayId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Replays_FileName",
                table: "Replays",
                column: "FileName");

            migrationBuilder.CreateIndex(
                name: "IX_Replays_GameTime",
                table: "Replays",
                column: "GameTime");

            migrationBuilder.CreateIndex(
                name: "IX_Replays_GameTime_GameMode",
                table: "Replays",
                columns: new[] { "GameTime", "GameMode" });

            migrationBuilder.CreateIndex(
                name: "IX_Replays_GameTime_GameMode_DefaultFilter",
                table: "Replays",
                columns: new[] { "GameTime", "GameMode", "DefaultFilter" });

            migrationBuilder.CreateIndex(
                name: "IX_Replays_GameTime_GameMode_Maxleaver",
                table: "Replays",
                columns: new[] { "GameTime", "GameMode", "Maxleaver" });

            migrationBuilder.CreateIndex(
                name: "IX_Replays_GameTime_GameMode_WinnerTeam",
                table: "Replays",
                columns: new[] { "GameTime", "GameMode", "WinnerTeam" });

            migrationBuilder.CreateIndex(
                name: "IX_Replays_Imported",
                table: "Replays",
                column: "Imported");

            migrationBuilder.CreateIndex(
                name: "IX_Replays_Maxkillsum",
                table: "Replays",
                column: "Maxkillsum");

            migrationBuilder.CreateIndex(
                name: "IX_Replays_ReplayEventId",
                table: "Replays",
                column: "ReplayEventId");

            migrationBuilder.CreateIndex(
                name: "IX_Replays_ReplayHash",
                table: "Replays",
                column: "ReplayHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepPlayerRatings_ReplayPlayerId",
                table: "RepPlayerRatings",
                column: "ReplayPlayerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RepPlayerRatings_ReplayRatingInfoId",
                table: "RepPlayerRatings",
                column: "ReplayRatingInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Spawns_ReplayPlayerId",
                table: "Spawns",
                column: "ReplayPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_SpawnUnits_SpawnId",
                table: "SpawnUnits",
                column: "SpawnId");

            migrationBuilder.CreateIndex(
                name: "IX_SpawnUnits_UnitId",
                table: "SpawnUnits",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Units_Name",
                table: "Units",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Upgrades_Name",
                table: "Upgrades",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UploaderReplays_UploadersUploaderId",
                table: "UploaderReplays",
                column: "UploadersUploaderId");

            migrationBuilder.CreateIndex(
                name: "IX_Uploaders_AppGuid",
                table: "Uploaders",
                column: "AppGuid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArcadePlayerRatingChanges");

            migrationBuilder.DropTable(
                name: "ArcadeReplayDsPlayerRatings");

            migrationBuilder.DropTable(
                name: "BattleNetInfos");

            migrationBuilder.DropTable(
                name: "BonusDamages");

            migrationBuilder.DropTable(
                name: "ComboPlayerRatings");

            migrationBuilder.DropTable(
                name: "ComboReplayPlayerRatings");

            migrationBuilder.DropTable(
                name: "ComboReplayRatings");

            migrationBuilder.DropTable(
                name: "CommanderMmrs");

            migrationBuilder.DropTable(
                name: "DsAbilityDsUnit");

            migrationBuilder.DropTable(
                name: "DsPickBans");

            migrationBuilder.DropTable(
                name: "DsUpdates");

            migrationBuilder.DropTable(
                name: "DsUpgrades");

            migrationBuilder.DropTable(
                name: "Faqs");

            migrationBuilder.DropTable(
                name: "FaqVotes");

            migrationBuilder.DropTable(
                name: "FunStatMemories");

            migrationBuilder.DropTable(
                name: "IhSessionPlayers");

            migrationBuilder.DropTable(
                name: "MaterializedArcadeReplays");

            migrationBuilder.DropTable(
                name: "NoUploadResults");

            migrationBuilder.DropTable(
                name: "PlayerRatingChanges");

            migrationBuilder.DropTable(
                name: "PlayerUpgrades");

            migrationBuilder.DropTable(
                name: "ReplayArcadeMatches");

            migrationBuilder.DropTable(
                name: "ReplayDownloadCounts");

            migrationBuilder.DropTable(
                name: "ReplayViewCounts");

            migrationBuilder.DropTable(
                name: "RepPlayerRatings");

            migrationBuilder.DropTable(
                name: "SkipReplays");

            migrationBuilder.DropTable(
                name: "SpawnUnits");

            migrationBuilder.DropTable(
                name: "StreakInfos");

            migrationBuilder.DropTable(
                name: "UploaderReplays");

            migrationBuilder.DropTable(
                name: "ArcadePlayerRatings");

            migrationBuilder.DropTable(
                name: "ArcadeReplayDsPlayers");

            migrationBuilder.DropTable(
                name: "ArcadeReplayRatings");

            migrationBuilder.DropTable(
                name: "DsWeapons");

            migrationBuilder.DropTable(
                name: "DsAbilities");

            migrationBuilder.DropTable(
                name: "IhSessions");

            migrationBuilder.DropTable(
                name: "PlayerRatings");

            migrationBuilder.DropTable(
                name: "ReplayRatings");

            migrationBuilder.DropTable(
                name: "Spawns");

            migrationBuilder.DropTable(
                name: "Units");

            migrationBuilder.DropTable(
                name: "ArcadeReplays");

            migrationBuilder.DropTable(
                name: "DsUnits");

            migrationBuilder.DropTable(
                name: "ReplayPlayers");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Replays");

            migrationBuilder.DropTable(
                name: "Upgrades");

            migrationBuilder.DropTable(
                name: "Uploaders");

            migrationBuilder.DropTable(
                name: "ReplayEvents");

            migrationBuilder.DropTable(
                name: "Events");
        }
    }
}
