using dsstats.shared;
using s2protocol.NET;
using s2protocol.NET.Models;

namespace dsstats.parser;

public static partial class Parser
{
    public static Area Area1 { get; } = new Area(new Point(165, 187), new Point(147, 205), new Point(159, 217), new Point(177, 199));
    public static Area Area2 { get; } = new Area(new Point(189, 163), new Point(171, 181), new Point(183, 193), new Point(201, 175));
    public static Area Area3 { get; } = new Area(new Point(213, 139), new Point(195, 157), new Point(207, 169), new Point(225, 151));
    public static Area Area4 { get; } = new Area(new Point(48, 70), new Point(30, 88), new Point(42, 100), new Point(60, 82));
    public static Area Area5 { get; } = new Area(new Point(72, 46), new Point(54, 64), new Point(66, 76), new Point(84, 58));
    public static Area Area6 { get; } = new Area(new Point(96, 22), new Point(78, 40), new Point(90, 52), new Point(108, 34));
    public static Point Planetary { get; } = new Point(160, 152);
    public static Point Nexus { get; } = new Point(96, 88);


    public static ParseResult ParseReplay(Sc2Replay replay)
    {
        if (replay.Details?.Title != "Direct Strike")
        {
            return new()
            {
                Error = "No Direct Strike replay."
            };
        }

        ReplayDto replayDto = new()
        {
            FileName = replay.FileName,
            GameTime = DateTime.FromFileTimeUtc(replay.Details.TimeUTC),
            CommandersTeam1 = "",
            CommandersTeam2 = "",
            ReplayPlayers = GetReplayPlayers(replay.Details.Players, replay.Metadata)
        };

        if (replay.TrackerEvents is null)
        {
            return new()
            {
                ReplayDto = replayDto,
                Error = "no trackerevents found."
            };
        }

        try
        {
            ParseTrackerEvents(replayDto, replay.TrackerEvents);
        }
        catch (Exception ex)
        {
            return new()
            {
                Error = ex.Message
            };
        }

        return new()
        {
            ReplayDto = replayDto
        };
    }

    private static ICollection<ReplayPlayerDto> GetReplayPlayers(ICollection<DetailsPlayer> players, Metadata? metadata)
    {
        List<ReplayPlayerDto> replayPlayers = [];

        int failesafe_pos = 0;

        foreach (var player in players)
        {
            if (player.Observe > 0)
            {
                continue;
            }

            failesafe_pos++;
            
            MetadataPlayer? metaPlayer = null;
            if (metadata?.Players.Count >= failesafe_pos)
            {
                metaPlayer = metadata?.Players.ElementAt(failesafe_pos - 1);
            }

            Commander race = Commander.None;

            if (Enum.TryParse(typeof(Commander), player.Race, out var plRaceObj)
                && plRaceObj is Commander plRace)
            {
                race = plRace;
            }

            replayPlayers.Add(new ReplayPlayerDto()
            {
                Name = player.Name,
                Clan = player.ClanName,
                Race = race,
                APM = metaPlayer?.APM == null ? 0 : Convert.ToInt32(metaPlayer.APM),
                GamePos = player.WorkingSetSlotId == 0 ? failesafe_pos : player.WorkingSetSlotId,
                Player = new PlayerDto()
                {
                    Name = player.Name,
                    ToonId = player.Toon.Id,
                    RegionId = player.Toon.Region,
                    RealmId = player.Toon.Realm
                }
            });
        }
        return replayPlayers;
    }
}

public record ParseResult
{
    public ReplayDto? ReplayDto { get; set; }
    public string? Error { get; set; }
}


public record Point(int X, int Y)
{
    public static Point Zero = new(0, 0);
};
public record Area(Point South, Point West, Point North, Point East)
{
    public bool IsPointInside(Point point)
    {
        int windingNumber = ComputeWindingNumber(point);
        return windingNumber != 0;
    }

    public bool IsPointBetweenParallelLines(Point parallelLinePoint, Point checkPoint, int distance)
    {
        // Calculate vectors
        var vectorNE = new Point(East.X - North.X, East.Y - North.Y);
        var vectorNP = new Point(parallelLinePoint.X - North.X, parallelLinePoint.Y - North.Y);
        var vectorCP = new Point(checkPoint.X - North.X, checkPoint.Y - North.Y);

        // Normalize vectorNE
        double lengthNE = Math.Sqrt(vectorNE.X * vectorNE.X + vectorNE.Y * vectorNE.Y);
        vectorNE = new Point(Convert.ToInt32(vectorNE.X / lengthNE), Convert.ToInt32(vectorNE.Y / lengthNE));

        // Calculate projections
        double projectionCP = (vectorCP.X * vectorNE.X + vectorCP.Y * vectorNE.Y);
        double projectionNP = (vectorNP.X * vectorNE.X + vectorNP.Y * vectorNE.Y);

        // Move the lines toward each other
        projectionCP -= distance;
        projectionNP += distance;

        // Check if checkPoint is between the lines
        return projectionCP >= 0 && projectionCP <= 1 && projectionNP >= 0 && projectionNP <= 1;
    }

    private int ComputeWindingNumber(Point point)
    {
        if (IsLeft(North, East, point) 
            && IsLeft(East, South, point) 
            && IsLeft(South, West, point) 
            && IsLeft(West, North, point))
        {
            return 1;
        }
        return 0;
    }

    private bool IsLeft(Point a, Point b, Point c)
    {
        return ((b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y)) > 0;
    }

    public static Area Zero = new(Point.Zero, Point.Zero, Point.Zero, Point.Zero);
}


