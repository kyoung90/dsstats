
using dsstats.db8.AutoMapper;
using dsstats.db8;
using dsstats.db8services;
using dsstats.shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using dsstats.shared.Aram;

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

        for (int i = 0; i < 100; i++)
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
}
