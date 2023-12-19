using dsstats.db8;
using dsstats.parser;
using dsstats.shared;
using System.Text.Json;

namespace dsstats.cli;

public static class UnitPositions
{
    public static void CreatePosLayout(ReplayContext context)
    {
        Dictionary<int, string> unitNames = (context.Units
            .Select(s => new { s.UnitId, s.Name }))
            .ToDictionary(k => k.UnitId, v => v.Name);

        var query1 = from r in context.Replays
                    from rp in r.ReplayPlayers
                    from sp in rp.Spawns
                    from u in sp.Units
                    where r.GameTime > new DateTime(2023, 1, 22)
                     && (rp.ComboReplayPlayerRating != null && rp.ComboReplayPlayerRating.Rating >= 1500)
                     && sp.Breakpoint == shared.Breakpoint.Min5
                     && rp.Team == 1
                     && rp.Race == Commander.Fenix
                     && rp.OppRace == Commander.Dehaka
                    select u;

        var query2 = from r in context.Replays
                     from rp in r.ReplayPlayers
                     from sp in rp.Spawns
                     from u in sp.Units
                     where r.GameTime > new DateTime(2023, 1, 22)
                      && (rp.ComboReplayPlayerRating != null && rp.ComboReplayPlayerRating.Rating >= 1500)
                      && sp.Breakpoint == shared.Breakpoint.Min5
                      && rp.Team == 2
                      && rp.Race == Commander.Fenix
                      && rp.OppRace == Commander.Dehaka
                     select u;


        var units1 = query1.ToList();
        var units2 = query1.ToList();

        Area area1 = Parser.SpawnArea1;
        Area area2 = Parser.SpawnArea1;
        NormalizedArea normalizedArea1 = new(area1);
        NormalizedArea normalizedArea2 = new(area2);

        List<PointInfo> points1 = GetPointInfos(normalizedArea1, units1, unitNames);
        List<PointInfo> points2 = GetPointInfos(normalizedArea2, units2, unitNames);

        UnitMap unitMap1 = new()
        {
            Infos = points1,
        };
        UnitMap unitMap2 = new()
        {
            Infos = points2,
        };

        var json1 = JsonSerializer.Serialize(unitMap1, new JsonSerializerOptions() { WriteIndented = true });
        File.WriteAllText("/data/ds/unitmap1.json",  json1);
        var json2 = JsonSerializer.Serialize(unitMap2, new JsonSerializerOptions() { WriteIndented = true });
        File.WriteAllText("/data/ds/unitmap2.json", json2);
    }

    private static List<PointInfo> GetPointInfos(NormalizedArea normalizedArea, List<SpawnUnit> units, Dictionary<int, string> unitNames)
    {
        Dictionary<Point, Dictionary<string, int>> infos = new();

        foreach (var unit in units) 
        { 
            if (string.IsNullOrEmpty(unit.Poss))
            {
                continue;
            }
            var coords = unit.Poss.Split(',', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < coords.Length; i = i + 2)
            {
                Point point = new(int.Parse(coords[i]), int.Parse(coords[i + 1]));
                if (!normalizedArea.Area.IsPointInside(point))
                {
                    continue;
                }
                if (!infos.TryGetValue(point, out var info)
                    || info is null)
                {
                    info  = infos[point] = new();
                }

                var name = unitNames[unit.UnitId];

                if (!info.ContainsKey(name))
                {
                    info[name] = 1;
                }
                else
                {
                    info[name]++;
                }
            }
        }
        return infos.Select(s => new PointInfo()
        {
            Point = normalizedArea.GetNormalizedPoint(s.Key),
            UnitCounts = s.Value
        }).ToList();
    }
}


public record UnitMap
{
    public List<PointInfo> Infos { get; set; } = new();
}

public record PointInfo
{
    public Point Point { get; set; } = Point.Zero;
    public Dictionary<string, int> UnitCounts { get; set; } = new();
}