using dsstats.shared;

namespace dsstats.parser;

public static partial class Parser
{
    private static ReplayDto MapTrackerReplay(ReplayDto replayDto, TrackerReplay trackerReplay)
    {
        List<ReplayPlayerDto> replayPlayers = new();
        foreach (var ent in trackerReplay.PlayerMap)
        {
            var replayPlayer = replayDto.ReplayPlayers.FirstOrDefault(f => f.GamePos == ent.Key);

            if (replayPlayer is null)
            {
                throw new ArgumentOutOfRangeException(nameof(replayPlayer));
            }

            var mappedReplayPlayer = GetReplayPlayer(replayPlayer, ent.Value, trackerReplay);
            replayPlayers.Add(mappedReplayPlayer);
        }

        var allBps = replayPlayers.SelectMany(s => s.Spawns).Where(x => x.Breakpoint == Breakpoint.All);
        int duration = GetSecondsFromGameloop(trackerReplay.NexusOrCCDiedGameloop);

        replayDto = replayDto with
        {
            ReplayPlayers = replayPlayers,
            Duration = duration,
            CommandersTeam1 = '|' + string.Join('|', replayPlayers
                    .Where(x => x.GamePos <= 3).OrderBy(o => o.GamePos).Select(s => (int)s.Race)) + '|',
            CommandersTeam2 = '|' + string.Join('|', replayPlayers
                    .Where(x => x.GamePos > 3).OrderBy(o => o.GamePos).Select(s => (int)s.Race)) + '|',
            Cannon = GetSecondsFromGameloop(trackerReplay.CannonDown),
            Bunker = GetSecondsFromGameloop(trackerReplay.BunkerDown),
            Middle = GetMiddle(trackerReplay.MiddleChanges),
            Minkillsum = allBps.Min(m => m.KilledValue),
            Maxkillsum = allBps.Max(m => m.KilledValue),
            Minarmy = allBps.Min(m => m.ArmyValue),
            Minincome = allBps.Min(m => m.Income),
            Maxleaver = duration - replayPlayers.Min(m => m.Duration),
            Playercount = (byte)replayPlayers.Count,
        };

        return replayDto;
    }

    private static string GetMiddle(List<TrackerReplayMiddleChange> middleChanges)
    {
        if (middleChanges.Count == 0)
        {
            return "";
        }
        var firstChange = middleChanges.First();
        int startTeam = firstChange.Team;
        int startLoop = firstChange.Gameloop;

        return $"{startTeam}|{startLoop}|{string.Join('|', middleChanges.Skip(1).Select(s => s.Gameloop))}";
    }

    private static ReplayPlayerDto GetReplayPlayer(ReplayPlayerDto replayPlayer, TrackerPlayer trackerPlayer, TrackerReplay trackerReplay)
    {
        int team = trackerPlayer.GamePos <= 3 ? 1 : 2;
        var lastStats = trackerPlayer.Stats.OrderBy(o => o.Gameloop).LastOrDefault();
        var refineries = trackerPlayer.Refineries.Where(x => x.Gameloop > 0)
            .OrderBy(o => o.Gameloop)
            .ToList();
        var spawns = GetSpawns(trackerPlayer, trackerReplay.NexusOrCCDiedGameloop);

        replayPlayer = replayPlayer with
        {
            GamePos = trackerPlayer.GamePos,
            Team = team,
            PlayerResult = team == trackerReplay.WinnerTeam ? PlayerResult.Win : PlayerResult.Los,
            Duration = GetSecondsFromGameloop(trackerPlayer.Stats.OrderBy(f => f.Gameloop).LastOrDefault()?.Gameloop ?? 0),
            Race = trackerPlayer.Commander,
            OppRace = GetOppRace(trackerPlayer, trackerReplay),
            Income = trackerPlayer.Stats.Sum(s => s.Income),
            Army = (lastStats?.ArmyValue ?? 0) / 2,
            Kills = lastStats?.KilledValue ?? 0,
            UpgradesSpent = lastStats?.UpgradesSpent ?? 0,
            TierUpgrades = trackerPlayer.Tiers.Count == 0 ? "" : string.Join('|', trackerPlayer.Tiers.Select(s => s.Gameloop)),
            Refineries = refineries.Count == 0 ? "" : string.Join('|', refineries.Select(s => s.Gameloop)),
            Upgrades = trackerPlayer.Upgrades.Select(s => new PlayerUpgradeDto()
            {
                Upgrade = new UpgradeDto()
                {
                    Name = s.Name,
                },
                Gameloop = s.Gameloop
            }).ToList(),
            Spawns = spawns
        };

        return replayPlayer;
    }

    private static ICollection<SpawnDto> GetSpawns(TrackerPlayer trackerPlayer, int lastGameloop)
    {
        Dictionary<Breakpoint, Dictionary<string, List<Point>>> units = new();
        Dictionary<Breakpoint, int> maxGameLoops = new();
        Dictionary<int, int> armyValues = new();

        int lastloop = trackerPlayer.Units.FirstOrDefault()?.Gameloop ?? 0;

        foreach (var unit in trackerPlayer.Units.OrderBy(o => o.Gameloop))
        {
            if (unit.Gameloop - lastloop > 400)
            {
                lastGameloop = unit.Gameloop;
                var nextStat = trackerPlayer.Stats.FirstOrDefault(f => f.Gameloop > lastGameloop);
                if (nextStat is not null)
                {
                    armyValues[lastGameloop] = nextStat.ArmyValue / 2;
                }
            }

            var unitBreakpoint = GetBreakpoint(unit.Gameloop);

            if (unitBreakpoint == Breakpoint.None)
            {
                continue;
            }

            if (!units.TryGetValue(unitBreakpoint, out var list)
                || list is null)
            {
                list = units[unitBreakpoint] = new();
            }

            maxGameLoops[unitBreakpoint] = unit.Gameloop;

            if (!list.ContainsKey(unit.Name))
            {
                list[unit.Name] = [unit.Pos];
            }
            else
            {
                list[unit.Name].Add(unit.Pos);
            }
        }

        List<SpawnDto> spawns = new();
        foreach (var ent in units)
        {
            int maxGameloop = 0;
            if (maxGameLoops.ContainsKey(ent.Key))
            {
                maxGameloop = maxGameLoops[ent.Key];
            }
            else
            {
                continue;
            }

            var bpStat = trackerPlayer.Stats.OrderBy(o => o.Gameloop)
                .FirstOrDefault(f => f.Gameloop >= maxGameloop);


            spawns.Add(new()
            {
                Gameloop = maxGameloop,
                Breakpoint = ent.Key,
                Income = trackerPlayer.Stats.Where(x => x.Gameloop <= maxGameloop).Sum(s => s.Income),
                GasCount = trackerPlayer.Refineries.Where(x => x.Gameloop != 0 && x.Gameloop <= maxGameloop).Count(),
                ArmyValue = armyValues.Where(x => x.Key <= maxGameloop).Sum(s => s.Value),
                KilledValue = bpStat?.KilledValue ?? 0,
                UpgradeSpent = bpStat?.UpgradesSpent ?? 0,
                Units = ent.Value.Select(s => new SpawnUnitDto()
                {
                    Count = (byte)s.Value.Count,
                    Unit = new UnitDto()
                    {
                        Name = s.Key
                    },
                    Poss = string.Join(',', s.Value.Select(s => $"{s.X},{s.Y}"))
                }).ToList()
            });
        }

        var lastSpawn = GetLastSpawn(trackerPlayer, lastGameloop);
        lastSpawn = lastSpawn with { ArmyValue = armyValues.Sum(s => s.Value) };

        var dupSpawn = spawns.FirstOrDefault(f => f.ArmyValue == lastSpawn.ArmyValue);

        if (dupSpawn is not null)
        {
            spawns.Remove(dupSpawn);
            spawns.Add(dupSpawn with { Breakpoint = Breakpoint.All });
        }
        else
        {
            spawns.Add(lastSpawn);
        }

        return spawns;
    }

    private static SpawnDto GetLastSpawn(TrackerPlayer trackerPlayer, int lastGameloop)
    {
        Dictionary<string, List<Point>> units = new();

        int gameloop = 0;

        foreach (var unit in trackerPlayer.Units.OrderByDescending(o => o.Gameloop))
        {
            if (gameloop == 0)
            {
                gameloop = unit.Gameloop;
            }
            else if (gameloop - unit.Gameloop >= 400)
            {
                break;
            }
            if (!units.ContainsKey(unit.Name))
            {
                units[unit.Name] = [unit.Pos];
            }
            else
            {
                units[unit.Name].Add(unit.Pos);
            }
        }

        var lastStats = trackerPlayer.Stats.LastOrDefault(f => f.Gameloop < lastGameloop);

        return new()
        {
            Gameloop = lastStats?.Gameloop ?? 0,
            Breakpoint = Breakpoint.All,
            Income = trackerPlayer.Stats.Where(x => x.Gameloop <= lastGameloop).Sum(s => s.Income),
            GasCount = trackerPlayer.Refineries.Where(x => x.Gameloop != 0).Count(),
            KilledValue = lastStats?.KilledValue ?? 0,
            UpgradeSpent = lastStats?.UpgradesSpent ?? 0,
            Units = units.Select(s => new SpawnUnitDto()
            {
                Count = (byte)s.Value.Count,
                Unit = new UnitDto()
                {
                    Name = s.Key
                },
                Poss = string.Join(',', s.Value.Select(s => $"{s.X},{s.Y}"))
            }).ToList()
        };
    }


    private static Commander GetOppRace(TrackerPlayer player, TrackerReplay trackerReplay)
    {
        var oppPos = player.GamePos switch
        {
            1 => 4,
            2 => 5,
            3 => 6,
            4 => 1,
            5 => 2,
            6 => 3,
            _ => 0
        };

        var oppPlayer = trackerReplay.PlayerMap.Values
            .FirstOrDefault(f => f.GamePos == oppPos);

        if (oppPlayer is not null)
        {
            return oppPlayer.Commander;
        }
        else
        {
            return Commander.None;
        }
    }
}
