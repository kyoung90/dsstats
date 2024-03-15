using dsstats.shared;
using System.Security.Cryptography;
using dsstats.shared.Extensions;
using dsstats.db8;
using SC2ArcadeCrawler;

namespace dsstats.ratingsng.tests;

public static class TestHelper
{
    private static readonly List<RequestNames> playerPool = [];
    private static readonly List<UpgradeDto> upgradePool = [];
    private static readonly List<UnitDto> unitPool = [];

    public static void SeedPools(int poolCount = 100)
    {
        for (int i = 2; i < poolCount + 2; i++)
        {
            playerPool.Add(new($"Test{i}", i, 1, 1));
            upgradePool.Add(new()
            {
                Name = $"Upgrade{i}"
            });
            unitPool.Add(new()
            {
                Name = $"Unit{i}"
            });
        }
    }

    public static ReplayDto GetBasicReplayDto(MD5 md5, GameMode gameMode = GameMode.Commanders)
    {
        if (playerPool.Count == 0)
        {
            SeedPools();
        }

        var replay = new ReplayDto()
        {
            FileName = "",
            GameMode = gameMode,
            GameTime = DateTime.UtcNow,
            Duration = 500,
            WinnerTeam = 1,
            Minkillsum = Random.Shared.Next(100, 1000),
            Maxkillsum = Random.Shared.Next(10000, 20000),
            Minincome = Random.Shared.Next(1000, 2000),
            Minarmy = Random.Shared.Next(1000, 2000),
            CommandersTeam1 = gameMode == GameMode.Standard ? "|1|1|1|" : "|10|10|10|",
            CommandersTeam2 = "|10|10|10|",
            Playercount = 6,
            Middle = "",
            ReplayPlayers = GetBasicReplayPlayerDtos(gameMode).ToList()
        };
        replay.GenHash(md5);
        return replay;
    }

    private static  ReplayPlayerDto[] GetBasicReplayPlayerDtos(GameMode gameMode)
    {
        var players = GetDefaultPlayers();
        return players.Select((s, i) => new ReplayPlayerDto()
        {
            Name = "Test",
            GamePos = i + 1,
            Team = i + 1 <= 3 ? 1 : 2,
            PlayerResult = i + 1 <= 3 ? PlayerResult.Win : PlayerResult.Los,
            Duration = 500,
            Race = gameMode == GameMode.Standard ? Commander.Protoss : Commander.Abathur,
            OppRace = gameMode == GameMode.Standard ? Commander.Protoss : Commander.Abathur,
            Income = Random.Shared.Next(1500, 3000),
            Army = Random.Shared.Next(1500, 3000),
            Kills = Random.Shared.Next(1500, 3000),
            TierUpgrades = "",
            Refineries = "",
            Player = s,
            Upgrades = GetDefaultUpgrades().Select(s => new PlayerUpgradeDto()
            {
                Gameloop = Random.Shared.Next(10, 11200),
                Upgrade = s
            }).ToList(),
            Spawns = new List<SpawnDto>() { GetDefaultSpawn() }
        }).ToArray();
    }

    private static PlayerDto[] GetDefaultPlayers()
    {
        var defaultPlayerPool = playerPool.ToArray();
        Random.Shared.Shuffle(defaultPlayerPool);

        return defaultPlayerPool.Take(6)
            .Select(s => new PlayerDto()
            {
                Name = s.Name,
                ToonId = s.ToonId,
                RealmId = s.RealmId,
                RegionId = s.RegionId,
            })
            .ToArray();
    }

    private static List<UpgradeDto> GetDefaultUpgrades()
    {
        List<UpgradeDto> upgrades = new();
        for (int i = 0; i < 3; i++)
        {
            var upgrade = upgradePool[Random.Shared.Next(0, upgradePool.Count)];
            upgrades.Add(upgrade);
        }
        return upgrades;
    }

    private static  List<UnitDto> GetDefaultUnits()
    {
        List<UnitDto> units = new();
        for (int i = 0; i < Random.Shared.Next(3, 20); i++)
        {
            var unit = unitPool[Random.Shared.Next(0, unitPool.Count)];
            units.Add(unit);
        }
        return units;
    }

    private static SpawnDto GetDefaultSpawn()
    {
        var units = GetDefaultUnits();
        return new()
        {
            Gameloop = 11200,
            Breakpoint = Breakpoint.All,
            Income = Random.Shared.Next(1000, 3000),
            GasCount = Random.Shared.Next(0, 3),
            ArmyValue = Random.Shared.Next(3000, 6000),
            KilledValue = Random.Shared.Next(3000, 6000),
            UpgradeSpent = Random.Shared.Next(500, 1500),
            Units = units.Select(s => new SpawnUnitDto()
            {
                Count = (byte)Random.Shared.Next(1, 254),
                Poss = "1,2,3,4,5,6,7,8",
                Unit = s
            }).ToList()
        };
    }

    public static LobbyResult GetBasicArcadeLobbyResult(GameMode gameMode = GameMode.Commanders)
    {
        List<PlayerProfile> playerProfiles = GetDefaultPlayerProfiles();

        return new()
        {
            Id = Random.Shared.Next(1, 10000),
            RegionId = 1,
            BnetBucketId = Random.Shared.Next(1, 100000),
            BnetRecordId = Random.Shared.Next(1, 100000),
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
            ClosedAt = DateTime.UtcNow,
            Status = "",
            MapVariantMode = "3V3 Commanders",
            SlotsHumansTotal = 6,
            SlotsHumansTaken = 6,
            Match = new()
            {
                Result = 1,
                CompletedAt = DateTime.UtcNow,
                ProfileMatches = GetDefaultProfileMatches(playerProfiles),
            },
            Slots = GetDefaultSlots(playerProfiles)
        };
    }

    private static List<Slot> GetDefaultSlots(List<PlayerProfile> playerProfiles)
    {
        return playerProfiles.Select((p, i) => new Slot
        {
            Team = i < 3 ? 1 : 2,
            Profile = p,
        }).ToList();
    }

    private static List<ArcadePlayerResult> GetDefaultProfileMatches(List<PlayerProfile> playerProfiles)
    {
        return playerProfiles.Select((p, i) => new ArcadePlayerResult
        {
            Decision = i < 3 ? "win" : "los",
            Profile = p
        }).ToList();
    }

    private static List<PlayerProfile> GetDefaultPlayerProfiles()
    {
        var playerDtos = GetDefaultPlayers();

        return playerDtos.Select(s => new PlayerProfile()
        {
            RegionId = s.RegionId,
            RealmId = s.RealmId,
            ProfileId = s.ToonId,
            Name = s.Name,
        }).ToList();
    }
}
