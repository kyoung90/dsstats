
using System.Security.Cryptography;
using System.Text.Json;
using pax.dsstats.parser;
using s2protocol.NET;

namespace dsstats.decodecli;

public static class Tourney
{
    public static async Task CreateTourneyJsons(string tourneyPath)
    {
        var replays = Directory.GetFiles(tourneyPath, "*.SC2Replay", SearchOption.AllDirectories)
            .ToHashSet();
        var existingJsons = replays.Where(x => File.Exists(Path.ChangeExtension(x, "json"))).ToList();
        replays.ExceptWith(existingJsons);

        if (replays.Count == 0)
        {
            Console.Write("not new tourney replays found.");
            return;
        }

        replays = replays.Select(s => s.Replace("\\", "/")).ToHashSet();

        ReplayDecoder decoder = new(Program.assemblyPath);

        ReplayDecoderOptions options = new()
        {
            Initdata = true,
            Details = true,
            Metadata = true,
            MessageEvents = false,
            TrackerEvents = true,
            GameEvents = false,
            AttributeEvents = false
        };

        using var md5 = MD5.Create();

        await foreach (var result in decoder.DecodeParallelWithErrorReport(replays, 8, options))
        {
            if (result.Sc2Replay is not null)
            {
                var dsReplay = Parse.GetDsReplay(result.Sc2Replay);

                if (dsReplay is null)
                {
                    continue;
                }

                var replayDto = Parse.GetReplayDto(dsReplay, md5);
                var json = JsonSerializer.Serialize(replayDto);
                File.WriteAllText(Path.ChangeExtension(result.ReplayPath, "json"), json);
            }
            else
            {
                Console.Write(result.Exception);
            }
        }

        Console.Write($"{replays.Count} new tourney replay jsons created.");
    }

    public static async Task GetMessageEvents()
    {
        var replayPath = @"C:\Users\pax77\Documents\StarCraft II\Accounts\107095918\2-S2-1-226401\Replays\Multiplayer\Direct Strike TE (478).SC2Replay";

        ReplayDecoder decoder = new(Program.assemblyPath);

        ReplayDecoderOptions options = new()
        {
            Initdata = false,
            Details = false,
            Metadata = false,
            MessageEvents = true,
            TrackerEvents = false,
            GameEvents = false,
            AttributeEvents = false
        };

        var replay = await decoder.DecodeAsync(replayPath, options);

        if (replay is null || replay.ChatMessages is null || replay.PingMessages is null)
        {
            return;
        }

        foreach (var msg in replay.ChatMessages)
        {
            Console.WriteLine($"{TimeSpan.FromSeconds(msg.Gameloop / 22.4).ToString(@"mm\:ss")} {msg.UserId} {msg.Message}");
        }

        foreach (var ping in replay.PingMessages)
        {
            Console.WriteLine($"{TimeSpan.FromSeconds(ping.Gameloop / 22.4).ToString(@"mm\:ss")} {ping.UserId} {ping.X}|{ping.Y}");
        }

        var pingCountsPerUser = replay.PingMessages
            .GroupBy(ping => ping.UserId)
            .Select(group => new { UserId = group.Key, PingCount = group.Count() });

        // Printing the results
        foreach (var pingCount in pingCountsPerUser)
        {
            Console.WriteLine($"User {pingCount.UserId}: {pingCount.PingCount} pings");
        }
    }
}