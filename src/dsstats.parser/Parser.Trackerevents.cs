using dsstats.shared;
using s2protocol.NET.Models;

namespace dsstats.parser;

public static partial class Parser
{
    private static TrackerReplay ParseTrackerEvents(ReplayDto replayDto, TrackerEvents trackerEvents)
    {
        Dictionary<int, TrackerPlayer> playerMap = GetPlayerMap(trackerEvents.SPlayerSetupEvents);

        TrackerReplay? trackerReplay = InitTrackerReplay(playerMap,
            trackerEvents.SUnitBornEvents.Where(x => x.Gameloop == 0).ToList());

        ArgumentNullException.ThrowIfNull(trackerReplay, "init trackerevents failed.");

        SetUnitsAndCommander(trackerReplay, trackerEvents.SUnitBornEvents);
        SetUpgrades(trackerReplay, trackerEvents.SUpgradeEvents);
        SetStats(trackerReplay, trackerEvents.SPlayerStatsEvents);
        SetMiddleChanges(trackerReplay, trackerEvents.SUnitOwnerChangeEvents);
        SetRefinieries(trackerReplay, trackerEvents.SUnitTypeChangeEvents);

        return trackerReplay;
    }

    private static void SetUpgrades(TrackerReplay trackerReplay, ICollection<SUpgradeEvent> sUpgradeEvents)
    {
        foreach (var upgradeEvent in sUpgradeEvents)
        {
            if (upgradeEvent.Gameloop < 2)
            {
                continue;
            }

            var trackerPlayer = GetTrackerPlayer(trackerReplay.PlayerMap, upgradeEvent.PlayerId);

            if (trackerPlayer is null)
            {
                continue;
            }

            if (upgradeEvent.UpgradeTypeName.Equals("Tier2", StringComparison.Ordinal)
                || upgradeEvent.UpgradeTypeName.Equals("Tier3", StringComparison.Ordinal))
            {
                trackerPlayer.Tiers.Add(new()
                {
                    Gameloop = upgradeEvent.Gameloop,
                });
                continue;
            }

            if (FilterUpgrades(upgradeEvent.UpgradeTypeName, trackerPlayer.Commander))
            {
                continue;
            }

            trackerPlayer.Upgrades.Add(new()
            {
                Name = upgradeEvent.UpgradeTypeName,
                Gameloop = upgradeEvent.Gameloop,
            });
        }
    }

    private static void SetStats(TrackerReplay trackerReplay, ICollection<SPlayerStatsEvent> sPlayerStatsEvents)
    {
        foreach (SPlayerStatsEvent statsEvent in sPlayerStatsEvents)
        {
            if (statsEvent.MineralsCollectionRate == 0)
            {
                continue;
            }

            var trackerPlayer = GetTrackerPlayer(trackerReplay.PlayerMap, statsEvent.PlayerId);

            if (trackerPlayer is null)
            {
                continue;
            }

            trackerPlayer.Stats.Add(new()
            {
                Gameloop = statsEvent.Gameloop,
                Income = statsEvent.MineralsCollectionRate,
                ArmyValue = statsEvent.MineralsUsedActiveForces,
                KilledValue = statsEvent.MineralsKilledArmy,
                UpgradesSpent = statsEvent.MineralsUsedCurrentTechnology
            });
        }
    }

    private static void SetMiddleChanges(TrackerReplay trackerReplay, ICollection<SUnitOwnerChangeEvent> sUnitOwnerChangeEvents)
    {
        foreach (var changeEvent in sUnitOwnerChangeEvents.Where(x => x.UnitTagIndex == 20))
        {
            trackerReplay.MiddleChanges.Add(new()
            {
                Gameloop = changeEvent.Gameloop,
                Team = changeEvent.UpkeepPlayerId switch
                {
                    13 => 1,
                    14 => 2,
                    _ => 0
                }
            });
        }
    }

    private static void SetRefinieries(TrackerReplay trackerReplay, ICollection<SUnitTypeChangeEvent> sUnitTypeChangeEvents)
    {
        foreach (var player in trackerReplay.PlayerMap.Values)
        {
            foreach (var refinery in player.Refineries)
            {
                var changeEvent = sUnitTypeChangeEvents.FirstOrDefault(f => f.UnitTagIndex == refinery.UnitTagIndex
                    && f.UnitTagRecycle == refinery.UnitTagRecycle);

                if (changeEvent is not null)
                {
                    refinery.Gameloop = changeEvent.Gameloop == 0 ? 1 : changeEvent.Gameloop;
                }
            }
        }
    }

    private static void SetUnitsAndCommander(TrackerReplay trackerReplay, ICollection<SUnitBornEvent> sUnitBornEvents)
    {
        foreach (var bornEvent in sUnitBornEvents)
        {
            if (bornEvent.Gameloop == 0)
            {
                continue;
            }

            var trackerPlayer = GetTrackerPlayer(trackerReplay.PlayerMap, bornEvent.ControlPlayerId);

            if (trackerPlayer is null)
            {
                continue;
            }

            if (bornEvent.Gameloop < 1440 && bornEvent.UnitTypeName.StartsWith("Worker"))
            {
                trackerPlayer.Commander = GetCommanderFromWorker(bornEvent.UnitTypeName);
                continue;
            }

            Point pos = new(bornEvent.X, bornEvent.Y);

            if (!IsSpawnUnit(trackerPlayer, pos))
            {
                continue;
            }

            trackerPlayer.Units.Add(new()
            {
                Name = FixUnitName(bornEvent.UnitTypeName, trackerPlayer.Commander),
                Gameloop = bornEvent.Gameloop,
                Pos = pos
            });
        }
    }

    private static bool IsSpawnUnit(TrackerPlayer player, Point unitPos)
    {
        return player.GamePos <= 3 ?
            SpawnArea1.IsPointInside(unitPos) :
            SpawnArea2.IsPointInside(unitPos);
    }

    private static TrackerReplay? InitTrackerReplay(Dictionary<int, TrackerPlayer> playerMap,
                                                    List<SUnitBornEvent> sUnitBornEvents)
    {
        SUnitBornEvent? nexusBornEvent = null;
        SUnitBornEvent? planetaryBornEvent = null;
        SUnitBornEvent? cannonBornEvent = null;
        SUnitBornEvent? bunkerBornEvent = null;

        bool objectivesSet = false;

        foreach (var bornEvent in sUnitBornEvents)
        {
            if (!objectivesSet)
            {
                if (bornEvent.UnitTypeName.Equals("ObjectiveNexus", StringComparison.Ordinal))
                {
                    nexusBornEvent = bornEvent;
                    objectivesSet = (nexusBornEvent != null && planetaryBornEvent != null && cannonBornEvent != null && bunkerBornEvent != null);
                }
                else if (bornEvent.UnitTypeName.Equals("ObjectivePlanetaryFortress", StringComparison.Ordinal))
                {
                    planetaryBornEvent = bornEvent;
                    objectivesSet = (nexusBornEvent != null && planetaryBornEvent != null && cannonBornEvent != null && bunkerBornEvent != null);
                }
                else if (bornEvent.UnitTypeName.Equals("ObjectivePhotonCannon", StringComparison.Ordinal))
                {
                    cannonBornEvent = bornEvent;
                    objectivesSet = (nexusBornEvent != null && planetaryBornEvent != null && cannonBornEvent != null && bunkerBornEvent != null);
                }
                else if (bornEvent.UnitTypeName.Equals("ObjectiveBunker", StringComparison.Ordinal))
                {
                    bunkerBornEvent = bornEvent;
                    objectivesSet = (nexusBornEvent != null && planetaryBornEvent != null && cannonBornEvent != null && bunkerBornEvent != null);
                }
            }

            var trackerPlayer = GetTrackerPlayer(playerMap, bornEvent.ControlPlayerId);

            if (trackerPlayer is null)
            {
                continue;
            }

            if (bornEvent.UnitTypeName.Equals("StagingAreaFootprintSouth", StringComparison.Ordinal) || bornEvent.UnitTypeName.Equals("AreaMarkerSouth", StringComparison.Ordinal))
            {
                trackerPlayer.StagingArea = trackerPlayer.StagingArea with { South = new(bornEvent.X, bornEvent.Y) };
            }
            else if (bornEvent.UnitTypeName.Equals("StagingAreaFootprintWest", StringComparison.Ordinal) || bornEvent.UnitTypeName.Equals("AreaMarkerWest", StringComparison.Ordinal))
            {
                trackerPlayer.StagingArea = trackerPlayer.StagingArea with { West = new(bornEvent.X, bornEvent.Y) };
            }
            else if (bornEvent.UnitTypeName.Equals("StagingAreaFootprintNorth", StringComparison.Ordinal) || bornEvent.UnitTypeName.Equals("AreaMarkerNorth", StringComparison.Ordinal))
            {
                trackerPlayer.StagingArea = trackerPlayer.StagingArea with { North = new(bornEvent.X, bornEvent.Y) };
            }
            else if (bornEvent.UnitTypeName.Equals("StagingAreaFootprintEast", StringComparison.Ordinal) || bornEvent.UnitTypeName.Equals("AreaMarkerEast", StringComparison.Ordinal))
            {
                trackerPlayer.StagingArea = trackerPlayer.StagingArea with { East = new(bornEvent.X, bornEvent.Y) };
            }
            else if (bornEvent.UnitTypeName.StartsWith("Worker"))
            {
                trackerPlayer.Commander = GetCommanderFromWorker(bornEvent.UnitTypeName);
            }
            else if (bornEvent.UnitTypeName.StartsWith("MineralField", StringComparison.Ordinal))
            {
                trackerPlayer.Refineries.Add(new()
                {
                    UnitTagIndex = bornEvent.UnitTagIndex,
                    UnitTagRecycle = bornEvent.UnitTagRecycle
                });
            }
        }

        if (nexusBornEvent == null || planetaryBornEvent == null
            || cannonBornEvent == null || bunkerBornEvent == null)
        {
            return null;
        }

        int duration = 0;
        int winnerTeam = 0;
        int bunkerDown = 0;
        int cannonDown = 0;
        if (nexusBornEvent.SUnitDiedEvent is not null)
        {
            duration = nexusBornEvent.SUnitDiedEvent.Gameloop;
            winnerTeam = 1;
        }
        else if (planetaryBornEvent.SUnitDiedEvent is not null)
        {
            duration = planetaryBornEvent.SUnitDiedEvent.Gameloop;
            winnerTeam = 2;
        }
        if (cannonBornEvent.SUnitDiedEvent is not null)
        {
            cannonDown = cannonBornEvent.SUnitDiedEvent.Gameloop;
        }
        if (bunkerBornEvent.SUnitDiedEvent is not null)
        {
            bunkerDown = bunkerBornEvent.SUnitDiedEvent.Gameloop;
        }

        var replay = new TrackerReplay()
        {
            Nexus = new(nexusBornEvent.X, nexusBornEvent.Y),
            Planetary = new(planetaryBornEvent.X, planetaryBornEvent.Y),
            Cannon = new(cannonBornEvent.X, cannonBornEvent.Y),
            Bunker = new(bunkerBornEvent.X, bunkerBornEvent.Y),
            PlayerMap = playerMap,
            NexusOrCCDiedGameloop = duration,
            WinnerTeam = winnerTeam,
            BunkerDown = bunkerDown,
            CannonDown = cannonDown,
        };

        SetObjectivesAndPlayerPos(replay);

        return replay;
    }

    private static void SetObjectivesAndPlayerPos(TrackerReplay replay)
    {
        if (replay.Nexus != Nexus)
        {
            // todo: set objectives
            throw new ArgumentOutOfRangeException(nameof(replay.Nexus));
        }

        foreach (var player in replay.PlayerMap.Values)
        {
            player.GamePos = player.StagingArea switch
            {
                _ when player.StagingArea == Area1 => 1,
                _ when player.StagingArea == Area2 => 2,
                _ when player.StagingArea == Area3 => 3,
                _ when player.StagingArea == Area4 => 4,
                _ when player.StagingArea == Area5 => 5,
                _ when player.StagingArea == Area6 => 6,
                _ => 0
            };
        }
    }

    private static Commander GetCommanderFromWorker(string unitTypeName)
    {
        var raceStr = unitTypeName[6..];
        if (Enum.TryParse(typeof(Commander), raceStr, out var raceObj)
            && raceObj is Commander race)
        {
            return race;
        }
        return Commander.None;
    }

    private static TrackerPlayer? GetTrackerPlayer(Dictionary<int, TrackerPlayer> playerMap, int playerId)
    {
        if (playerId <= 0 || playerId > 6)
        {
            return null;
        }

        if (playerMap.ContainsKey(playerId))
        {
            return playerMap[playerId];
        }
        return null;
    }

    private static Dictionary<int, TrackerPlayer> GetPlayerMap(ICollection<SPlayerSetupEvent> sPlayerSetupEvents)
    {
        Dictionary<int, TrackerPlayer> playerMap = new();

        foreach (var player in sPlayerSetupEvents)
        {
            playerMap[player.PlayerId] = new();
        }

        return playerMap;
    }

    internal record TrackerPlayer
    {
        public Area StagingArea { get; set; } = Area.Zero;
        public Commander Commander { get; set; }
        public int GamePos { get; set; }
        public List<TrackerPlayerUnit> Units { get; set; } = new();
        public List<TrackerPlayerRefinery> Refineries { get; set; } = new();
        public List<TrackerPlayerStat> Stats { get; set; } = new();
        public List<TrackerPlayerTiers> Tiers { get; set; } = new();
        public List<TrackerPlayerUpgrades> Upgrades { get; set; } = new();
    }

    internal record TrackerPlayerTiers
    {
        public int Gameloop { get; set; }
    }

    internal record TrackerPlayerUpgrades
    {
        public string Name { get; set; } = string.Empty;
        public int Gameloop { get; set; }
    }


    internal record TrackerPlayerStat
    {
        public int Gameloop { get; set; }
        public int Income { get; set; }
        public int ArmyValue { get; set; }
        public int KilledValue { get; set; }
        public int UpgradesSpent { get; set; }
    }

    internal record TrackerPlayerUnit
    {
        public string Name { get; set; } = string.Empty;
        public int Gameloop { get; set; }
        public Point Pos { get; set; } = Point.Zero;
    }

    internal record TrackerPlayerRefinery
    {
        public int UnitTagIndex { get; set; }
        public int UnitTagRecycle { get; set; }
        public int Gameloop { set; get; }
    }

    internal record TrackerReplay
    {
        public Area SpawnArea1 { get; set; } = Area.Zero;
        public Area SpawnArea2 { get; set; } = Area.Zero;
        public Point Nexus { get; init; } = Point.Zero;
        public Point Planetary { get; init; } = Point.Zero;
        public Point Cannon { get; init; } = Point.Zero;
        public Point Bunker { get; init; } = Point.Zero;
        public int CannonDown { get; init; }
        public int BunkerDown { get; init; }
        public Dictionary<int, TrackerPlayer> PlayerMap { get; set; } = new();
        public List<TrackerReplayMiddleChange> MiddleChanges { get; set; } = new();
        public int NexusOrCCDiedGameloop {  get; set; }
        public int WinnerTeam { get; set; }
    }

    internal record TrackerReplayMiddleChange
    {
        public int Team { get; set; }
        public int Gameloop { get; set; }
    }
}
