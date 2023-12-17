using dsstats.shared;
using s2protocol.NET.Models;

namespace dsstats.parser;

public static partial class Parser
{

    private static void ParseTrackerEvents(ReplayDto replayDto, TrackerEvents trackerEvents)
    {
        Dictionary<int, TrackerPlayer> playerMap = GetPlayerMap(trackerEvents.SPlayerSetupEvents);

        TrackerReplay? trackerReplay = InitTrackerReplay(playerMap,
            trackerEvents.SUnitBornEvents.Where(x => x.Gameloop == 0).ToList());

        ArgumentNullException.ThrowIfNull(trackerReplay, "init trackerevents failed.");

        SetUnits(trackerReplay, trackerEvents.SUnitBornEvents);

    }

    private static void SetUnits(TrackerReplay trackerReplay, ICollection<SUnitBornEvent> sUnitBornEvents)
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
                Name = bornEvent.UnitTypeName,
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
        }

        if (nexusBornEvent == null || planetaryBornEvent == null
            || cannonBornEvent == null || bunkerBornEvent == null)
        {
            return null;
        }

        foreach (var player in playerMap.Values)
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

        return new()
        {
            Nexus = new(nexusBornEvent.X, nexusBornEvent.Y),
            Planetary = new(planetaryBornEvent.X, planetaryBornEvent.Y),
            Cannon = new(cannonBornEvent.X, cannonBornEvent.Y),
            Bunker = new(bunkerBornEvent.X, bunkerBornEvent.Y),
            PlayerMap = playerMap
        };
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
    }

    internal record TrackerPlayerUnit
    {
        public string Name { get; set; } = string.Empty;
        public int Gameloop { get; set; }
        public Point Pos { get; set; } = Point.Zero;
        public UnitType UnitType { get; set; }
    }

    internal record TrackerReplay
    {
        public Point Nexus { get; init; } = Point.Zero;
        public Point Planetary { get; init; } = Point.Zero;
        public Point Cannon { get; init; } = Point.Zero;
        public Point Bunker { get; init; } = Point.Zero;
        public Dictionary<int, TrackerPlayer> PlayerMap { get; set; } = new();
    }
}

public enum UnitType
{
    None = 0,
    Build = 1,
    Spawn = 2,
    Tier = 3
}
