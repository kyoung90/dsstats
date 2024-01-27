using dsstats.db8;
using dsstats.db8.AutoMapper;
using dsstats.db8services;
using dsstats.db8services.Tourneys;
using dsstats.shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;


namespace dsstats.ratings.tests;

[TestClass]
public class TourneyTests
{
    private ServiceProvider serviceProvider;
    private readonly List<RequestNames> playerPool;
    private readonly int poolCount = 100;

    public TourneyTests()
    {
        playerPool = new();
        for (int i = 2; i < poolCount + 2; i++)
        {
            playerPool.Add(new($"Test{i}", i, 1, 1));
        }


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

        services.AddScoped<TeamsCreateService>();

        services.AddScoped<IReplayRepository, ReplayRepository>();

        serviceProvider = services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task T01CreateTourneyTest()
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ReplayContext>();
        var tourneyService = scope.ServiceProvider.GetRequiredService<TeamsCreateService>();
        context.Database.EnsureDeleted();
        context.Database.Migrate();


        TourneyCreateDto createDto = new()
        {
            Name = "TestTournament",
            EventStart = DateTime.Today,
            GameMode = GameMode.Standard
        };

        var result = await tourneyService.CreateTournament(createDto);
        Assert.IsFalse(result == Guid.Empty);
    }

    [TestMethod]
    public async Task T02AddPlayersTest()
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ReplayContext>();
        var tourneyService = scope.ServiceProvider.GetRequiredService<TeamsCreateService>();

        var tourney = context.Tourneys.FirstOrDefault();
        Assert.IsNotNull(tourney);

        var players = playerPool.Select(s => new Player()
        {
            Name = s.Name,
            ToonId = s.ToonId,
            RealmId = s.RealmId,
            RegionId = s.RegionId,
            ComboPlayerRatings = new List<ComboPlayerRating>()
            {
                new ComboPlayerRating()
                {
                    RatingType = RatingType.Std,
                    Rating = Random.Shared.Next(500, 2500)
                }
            }
        });

        context.Players.AddRange(players);
        context.SaveChanges();

        var result = await tourneyService.AddTournamentPlayers(new()
        {
            TourneyGuid = tourney.TourneyGuid,
            PlayerIds = players.Take(30).Select(s => new PlayerId(s.ToonId, s.RealmId, s.RegionId)).ToList()
        });

        Assert.IsTrue(result);

        var tourneyAfter = context.Tourneys
            .Include(i => i.TourneyPlayers)
            .FirstOrDefault(i => i.TourneyGuid ==  tourney.TourneyGuid);

        Assert.AreEqual(30, tourneyAfter?.TourneyPlayers.Count());
    }

    [TestMethod]
    public async Task T03CreateTeamsTest()
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ReplayContext>();
        var tourneyService = scope.ServiceProvider.GetRequiredService<TeamsCreateService>();

        var tourney = context.Tourneys.FirstOrDefault();
        Assert.IsNotNull(tourney);

        var result = await tourneyService.CreateRandomTeams(tourney.TourneyGuid, RatingType.Std);

        Assert.IsTrue(result);

        var teams = await context.TourneyTeams
            .Where(x => x.Tourney!.TourneyGuid == tourney.TourneyGuid)
            .ToListAsync();

        Assert.AreEqual(10,  teams.Count());
    }
}
