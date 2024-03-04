
using dsstats.db8.AutoMapper;
using dsstats.db8;
using dsstats.db8services;
using dsstats.shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using dsstats.shared.Aram;
using dsstats.db8.Aram;
using System.Security.Cryptography;
using dsstats.shared.Extensions;
using dsstats.db8services.Import;
using dsstats.shared.Interfaces;
using dsstats.api.Services;

namespace dsstats.ratings.tests.Aram;

[TestClass]
public class AramTests
{
    private ServiceProvider serviceProvider;

    public AramTests()
    {
        var services = new ServiceCollection();
        var serverVersion = new MySqlServerVersion(new Version(5, 7, 44));
        var jsonStrg = File.ReadAllText("/data/localserverconfig.json");
        var json = JsonSerializer.Deserialize<JsonElement>(jsonStrg);
        var config = json.GetProperty("ServerConfig");
        var connectionString = config.GetProperty("TestConnectionString").GetString();
        var importConnectionString = config.GetProperty("ImportTestConnectionString").GetString() ?? "";

        services.AddOptions<DbImportOptions>()
            .Configure(x =>
            {
                x.ImportConnectionString = importConnectionString;
                x.IsSqlite = false;
            });

        services.AddDbContext<ReplayContext>(options =>
        {
            options.UseMySql(connectionString, serverVersion, p =>
            {
                p.CommandTimeout(300);
                p.MigrationsAssembly("MysqlMigrations");
                p.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
            });
        });

        services.AddLogging();
        services.AddMemoryCache();
        services.AddAutoMapper(typeof(AutoMapperProfile));

        services.AddScoped<AramService>();

        services.AddSingleton<IRatingService, RatingService>();
        services.AddSingleton<IRatingsSaveService, RatingsSaveService>();
        services.AddSingleton<IRemoteToggleService, RemoteToggleService>();
        services.AddSingleton<ImportService>();

        serviceProvider = services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task T01CreateAramEventTest()
    {
        using var scope = serviceProvider.CreateScope();
        var aramService = scope.ServiceProvider.GetRequiredService<AramService>();
        var context = scope.ServiceProvider.GetRequiredService<ReplayContext>();

        context.Database.EnsureDeleted();
        context.Database.Migrate();

        AramEventDto eventDto = new()
        {
            Name = "Test1",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(6),
            GameMode = GameMode.Standard
        };

        await aramService.CreateAramEvent(eventDto);

        var aramEvent = await context.AramEvents.FirstOrDefaultAsync();
        Assert.IsNotNull(aramEvent);
        Assert.AreEqual("Test1", aramEvent.Name);
    }

    [TestMethod]
    public async Task T02AddPlayerTest()
    {
        using var scope = serviceProvider.CreateScope();
        var aramService = scope.ServiceProvider.GetRequiredService<AramService>();
        var context = scope.ServiceProvider.GetRequiredService<ReplayContext>();

        var eventGuid = await context.AramEvents
            .Select(s => s.Guid)
            .FirstOrDefaultAsync();

        var playerDto = new AramPlayerDto()
        {
            Name = "Test1",
            StartRating = 1000
        };

        await aramService.AddPlayer(eventGuid, playerDto);

        var player = await context.AramPlayers.FirstOrDefaultAsync();
        Assert.IsNotNull(player);
        Assert.AreEqual("Test1", player.Name);
    }

    [TestMethod]
    public async Task T03CreateStdMatchesTest()
    {
        using var scope = serviceProvider.CreateScope();
        var aramService = scope.ServiceProvider.GetRequiredService<AramService>();
        var context = scope.ServiceProvider.GetRequiredService<ReplayContext>();

        AramEventDto eventDto = new()
        {
            Name = "Test2",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(6),
            GameMode = GameMode.Standard
        };

        var eventGuid = await aramService.CreateAramEvent(eventDto);

        for (int i = 0; i < 14; i++)
        {
            var playerDto = new AramPlayerDto()
            {
                Name = $"Test{i + 2}",
                StartRating = Random.Shared.Next(1500, 2501)
            };
            await aramService.AddPlayer(eventGuid, playerDto);
        }

        await aramService.CreateMatches(eventGuid);

        var matches = await context.AramMatches
            .AsNoTracking()
            .ToListAsync();

        Assert.IsTrue(matches.Count > 0);
    }

    [TestMethod]
    public async Task T04CreateCmdrMatchesTest()
    {
        using var scope = serviceProvider.CreateScope();
        var aramService = scope.ServiceProvider.GetRequiredService<AramService>();
        var context = scope.ServiceProvider.GetRequiredService<ReplayContext>();

        AramEventDto eventDto = new()
        {
            Name = "Test3",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(6),
            GameMode = GameMode.Commanders
        };

        var eventGuid = await aramService.CreateAramEvent(eventDto);

        for (int i = 0; i < 14; i++)
        {
            var playerDto = new AramPlayerDto()
            {
                Name = $"Test{i + 2}",
                StartRating = Random.Shared.Next(1500, 3001)
            };
            await aramService.AddPlayer(eventGuid, playerDto);
        }

        await aramService.CreateMatches(eventGuid);

        var matches = await context.AramMatches
            .AsNoTracking()
            .ToListAsync();

        Assert.IsTrue(matches.Count > 0);
    }

    [TestMethod]
    public async Task T05MatchHistoryScoreTest()
    {
        using var scope = serviceProvider.CreateScope();
        var aramService = scope.ServiceProvider.GetRequiredService<AramService>();
        var context = scope.ServiceProvider.GetRequiredService<ReplayContext>();

        AramEventDto eventDto = new()
        {
            Name = "Test4",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(6),
            GameMode = GameMode.Commanders
        };

        var eventGuid = await aramService.CreateAramEvent(eventDto);

        for (int i = 0; i < 14; i++)
        {
            var playerDto = new AramPlayerDto()
            {
                Name = $"Test{i + 2}",
                StartRating = Random.Shared.Next(1500, 3001)
            };
            await aramService.AddPlayer(eventGuid, playerDto);
        }

        for (int i = 0; i < 10; i++)
        {
            await aramService.CreateMatches(eventGuid);
        }

        var matches = await context.AramMatches
            .Include(i => i.AramSlots)
            .AsNoTracking()
            .ToListAsync();

        Assert.IsTrue(matches.Count > 0);

        foreach (var match in matches)
        {
            HashSet<int> playerIds = match.AramSlots.Select(s => s.AramPlayerId).ToHashSet();
            Assert.AreEqual(6, playerIds.Count);
        }
    }

    [TestMethod]
    public async Task T06ReportMatchTest()
    {
        using var scope = serviceProvider.CreateScope();
        var aramService = scope.ServiceProvider.GetRequiredService<AramService>();
        var context = scope.ServiceProvider.GetRequiredService<ReplayContext>();
        var impoertService = scope.ServiceProvider.GetRequiredService<ImportService>();

        AramEventDto eventDto = new()
        {
            Name = "Test5",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(6),
            GameMode = GameMode.Commanders
        };

        var eventGuid = await aramService.CreateAramEvent(eventDto);

        var players = await CreateTestPlayers(14, context);
        var amPlayers = players.Where(x => x.RegionId == 1).ToList();
        var euPlayers = players.Where(x => x.RegionId == 2).ToList();

        for (int i = 0; i < 14; i++)
        {
            var playerDto = new AramPlayerDto()
            {
                Name = $"Test{i + 2}",
                StartRating = Random.Shared.Next(1500, 3001),
                AmPlayerId = amPlayers[i].PlayerId,
                EuPlayerId = euPlayers[i].PlayerId,
            };
            await aramService.AddPlayer(eventGuid, playerDto);
        }

        await aramService.CreateMatches(eventGuid);

        var match = await context.AramMatches
            .Include(i => i.AramSlots)
                .ThenInclude(i => i.AramPlayer)
            .Where(x => x.AramEvent!.Guid == eventGuid
                && x.MatchResult == MatchResult.None)
            .FirstOrDefaultAsync();

        Assert.IsNotNull(match);

        var replay1 = await CreateReplay(match, eventDto.GameMode, context);
        var replay2 = await CreateReplay(match, eventDto.GameMode, context);

        await impoertService.Import([replay1, replay2]);

        await aramService.AssignReplays();

        var assignedMatch = await context.AramMatches
            .FirstOrDefaultAsync(f => f.Guid == match.Guid);

        Assert.IsTrue(assignedMatch?.MatchResult != MatchResult.None);
    }

    private static async Task<ReplayDto> CreateReplay(AramMatch match, GameMode gameMode, ReplayContext context)
    {
        var md5 = MD5.Create();
        var replay = new ReplayDto()
        {
            FileName = "",
            GameMode = gameMode,
            GameTime = DateTime.UtcNow,
            Duration = 500,
            TournamentEdition = true,
            WinnerTeam = Random.Shared.Next(1, 3),
            Minkillsum = Random.Shared.Next(100, 1000),
            Maxkillsum = Random.Shared.Next(10000, 20000),
            Minincome = Random.Shared.Next(1000, 2000),
            Minarmy = Random.Shared.Next(1000, 2000),
            CommandersTeam1 = gameMode == GameMode.Standard ? "|1|1|1|" : "|10|10|10|",
            CommandersTeam2 = "|10|10|10|",
            Playercount = 6,
            Middle = "",
            ReplayPlayers = await GetBasicReplayPlayerDtos(gameMode, match, context)
        };
        replay.GenHash(md5);
        return replay;
    }

    private static async Task<ReplayPlayerDto[]> GetBasicReplayPlayerDtos(GameMode gameMode, AramMatch match, ReplayContext context)
    {
        var playerIds = match.AramSlots.Select(s => s.AramPlayer!.EuPlayerId).ToList();
        var players = await context.Players.Where(x => playerIds.Contains(x.PlayerId))
            .Select(s => new PlayerDto()
            {
                Name = s.Name,
                ToonId = s.ToonId,
                RealmId = s.RealmId,
                RegionId = s.RegionId,
            })
            .ToListAsync();

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
        }).ToArray();
    }

    private static async Task<List<Player>> CreateTestPlayers(int count, ReplayContext context)
    {
        List<Player> players = [];
        for (int i = 0; i < count; i++)
        {
            players.Add(new()
            {
                Name = $"Test{i + 1}",
                ToonId = 10 + i,
                RealmId = 1,
                RegionId = 1,
            });
            players.Add(new()
            {
                Name = $"Test{i + 2000 + 1}",
                ToonId = 2000 + i,
                RealmId = 1,
                RegionId = 2,
            });
        }
        context.Players.AddRange(players);
        await context.SaveChangesAsync();
        return players;
    }
}
