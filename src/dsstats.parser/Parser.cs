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
    public static Area SpawnArea1 { get; } = new Area(new(173, 147), new(155, 165), new(167, 177), new(185, 159));
    public static Area SpawnArea2 { get; } = new Area(new(89, 63), new(71, 81), new(83, 93), new(101, 75));

    public static ParseResult ParseReplay(Sc2Replay replay)
    {
        if (replay.Details is null)
        {
            return new()
            {
                Error = "no details found."
            };
        }

        if (!replay.Details.Title.Equals("Direct Strike", StringComparison.Ordinal))
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
            var trackerReplay = ParseTrackerEvents(replayDto, replay.TrackerEvents);
            replayDto = MapTrackerReplay(replayDto, trackerReplay);

            // todo lastspawn / replay stats
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
        bool fsActive = players.All(a => a.WorkingSetSlotId == 0);

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
                GamePos = fsActive ? failesafe_pos : player.WorkingSetSlotId + 1,
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

    private static int GetSecondsFromGameloop(int gameloop)
    {
        return Convert.ToInt32(gameloop / 22.4);
    }

    private static int GetGameloopFromSeconds(int seconds)
    {
        return Convert.ToInt32(seconds * 22.4);
    }

    private static Breakpoint GetBreakpoint(int gameloop)
    {
        return gameloop switch
        {
            _ when gameloop >= 6240 && gameloop <= 7209 => Breakpoint.Min5,
            _ when gameloop >= 12960 && gameloop <= 13928 => Breakpoint.Min10,
            _ when gameloop >= 19680 && gameloop <= 20649 => Breakpoint.Min15,
            _ => Breakpoint.None
        };

        //if (gameloop >= 6240 && gameloop < 7209)
        //(gameloop >= 12960 && gameloop < 13928)
        //(gameloop >= 19680 && gameloop < 20649)))
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

public sealed record Area
{
    public Area(Point south, Point west, Point north, Point east)
    {
        South = south;
        West = west;
        North = north;
        East = east;
        rectangleArea = CalculateArea();
    }

    public Point South { get; init; }
    public Point West { get; init; }
    public Point North { get; init; }
    public Point East { get; init; }

    private double rectangleArea;

    private double CalculateArea()
    {
        double area = 0.5 * Math.Abs(
            (North.X * East.Y + East.X * South.Y + South.X * West.Y + West.X * North.Y) -
            (East.X * North.Y + South.X * East.Y + West.X * South.Y + North.X * West.Y)
        );

        return area;
    }

    public bool IsPointInside(Point point)
    {
        var triangle1 = CalculateTriangleArea(North, East, point);
        var triangle2 = CalculateTriangleArea(East, South, point);
        var triangle3 = CalculateTriangleArea(South, West, point);
        var triangle4 = CalculateTriangleArea(West, North, point);

        return Math.Abs(rectangleArea - (triangle1 + triangle2 + triangle3 + triangle4)) < 1e-10;
    }

    private static double CalculateTriangleArea(Point pointA, Point pointB, Point pointC)
    {
        return 0.5 * Math.Abs(
            (pointA.X * (pointB.Y - pointC.Y) + pointB.X * (pointC.Y - pointA.Y) + pointC.X * (pointA.Y - pointB.Y))
        );
    }

    private int Sum(Point a, Point b, Point c)
    {
        return Math.Abs((b.X * a.Y - a.X * b.Y) + (c.X * b.Y - b.X * c.Y) + (a.X * c.X - c.X * a.Y)) / 2;
    }

    public Area MoveTowards(Point targetPoint)
    {
        // Calculate the translation vector
        int deltaX = targetPoint.X - Center().X;
        int deltaY = targetPoint.Y - Center().Y;

        // Translate each vertex of the rectangle
        var south = new Point(South.X + deltaX, South.Y + deltaY);
        var west = new Point(West.X + deltaX, West.Y + deltaY);
        var north = new Point(North.X + deltaX, North.Y + deltaY);
        var east = new Point(East.X + deltaX, East.Y + deltaY);

        return new(south, west, north, east);
    }

    private Point Center()
    {
        double centerX = (South.X + North.X) / 2.0;
        double centerY = (West.Y + East.Y) / 2.0;
        return new Point(Convert.ToInt32(centerX), Convert.ToInt32(centerY));
    }

    public static Point Midpoint(Point A, Point L1, Point L2)
    {
        double midX = (A.X + (L1.X + L2.X) / 2.0) / 2.0;
        double midY = (A.Y + (L1.Y + L2.Y) / 2.0) / 2.0;
        return new Point(Convert.ToInt32(midX), Convert.ToInt32(midY));
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


    private static readonly Area _zero = new(Point.Zero, Point.Zero, Point.Zero, Point.Zero);
    public static Area Zero => _zero;

    public bool Equals(Area? other)
    {
        return (South == other?.South && West == other?.West && North == other?.North && East == other?.East);
    }

    public override int GetHashCode()
    {
        return (South.GetHashCode() + West.GetHashCode() + North.GetHashCode() + East.GetHashCode());
    }
}


