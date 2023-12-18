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
        replayDto = replayDto with
        {
            ReplayPlayers = replayPlayers,
            Duration = GetSecondsFromGameloop(trackerReplay.NexusOrCCDiedGameloop),
            CommandersTeam1 = '|' + string.Join('|', replayPlayers
                    .Where(x => x.GamePos <= 3).OrderBy(o => o.GamePos).Select(s => (int)s.Race)) + '|',
            CommandersTeam2 = '|' + string.Join('|', replayPlayers
                    .Where(x => x.GamePos > 3).OrderBy(o => o.GamePos).Select(s => (int)s.Race)) + '|',
            Cannon = trackerReplay.CannonDown,
            Bunker = trackerReplay.BunkerDown,
            Middle = GetMiddle(trackerReplay.MiddleChanges)
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
            Spawns = GetSpawns(trackerPlayer)
        };



        return replayPlayer;
    }

    private static ICollection<SpawnDto> GetSpawns(TrackerPlayer trackerPlayer)
    {
        Dictionary<Breakpoint, Dictionary<string, List<Point>>> units = new();
        Dictionary<Breakpoint, int> maxGameLoops = new();

        foreach (var unit in trackerPlayer.Units.OrderBy(o => o.Gameloop))
        {
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
                GasCount = trackerPlayer.Refineries.Where(x => x.Gameloop <= maxGameloop).Count(),
                ArmyValue = (bpStat?.ArmyValue ?? 0) / 2,
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
        return spawns;
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
