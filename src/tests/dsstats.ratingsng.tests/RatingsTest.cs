using dsstats.db8.AutoMapper;
using dsstats.db8;
using dsstats.ratings;
using dsstats.shared.Interfaces;
using dsstats.shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using dsstats.db8services.Import;
using dsstats.api.Services;
using dsstats.db8services;
using System.Security.Cryptography;
using SC2ArcadeCrawler;

namespace dsstats.ratingsng.tests
{
    [TestClass]
    public class RatingsTest
    {
        private readonly ServiceProvider serviceProvider;

        public RatingsTest()
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
            services.AddHttpClient();

            services.AddSingleton<IRatingService, RatingService>();
            services.AddSingleton<IRatingsSaveService, RatingsSaveService>();
            services.AddSingleton<IImportService, ImportService>();
            services.AddSingleton<IRemoteToggleService, RemoteToggleService>();

            services.AddScoped<IReplayRepository, ReplayRepository>();
            services.AddScoped<CrawlerService>();

            services.AddRatings();

            serviceProvider = services.BuildServiceProvider();
        }

        [TestMethod]
        public void T01ImportRatingsTest()
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ReplayContext>();
            var importService = scope.ServiceProvider.GetRequiredService<IImportService>();
            var crawlerService = scope.ServiceProvider.GetRequiredService<CrawlerService>();

            context.Database.EnsureDeleted();
            context.Database.Migrate();

            using var md5 = MD5.Create();

            int replayCount = 100;
            List<ReplayDto> replayDtos = [];
            List<LobbyResult> lobbyResults = [];
            for (int i = 0; i < replayCount; i++)
            {
                replayDtos.Add(TestHelper.GetBasicReplayDto(md5));
                lobbyResults.Add(TestHelper.GetBasicArcadeLobbyResult());
            }
            importService.Import(replayDtos).Wait();
            crawlerService.ImportArcadeReplays(new(regionId: 1, mapId: 208271, handle: "2-S2-1-226401", teMap: false)
            {
                Results = lobbyResults
            }, default).Wait();

            var replayNgRatings = context.ReplayNgRatings.ToList();

            Assert.IsTrue(replayNgRatings.Count > 0);
            Assert.IsTrue(replayNgRatings.All(a => a.IsPreRating));

        }

        [TestMethod]
        public void T02ContinueRatingsTest()
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ReplayContext>();
            var comboRatingCalcService = scope.ServiceProvider.GetRequiredService<ComboRatingCalcService>();

            comboRatingCalcService.PrdoduceContinueRatings(new()
            {
                FromDate = DateTime.UtcNow.AddHours(-3),
                RatingType = RatingNgType.All,
                GameModes = [GameMode.Standard, GameMode.Commanders, GameMode.CommandersHeroic, GameMode.BrawlCommanders],
                Take = 100_000
            }).Wait();

            var replayNgRatings = context.ReplayNgRatings.ToList();
            var playerNgRatings = context.PlayerNgRatings.ToList();

            Assert.IsTrue(replayNgRatings.Count > 0);
            Assert.IsTrue(replayNgRatings.All(a => !a.IsPreRating));
            Assert.IsTrue(playerNgRatings.Count > 0);
        }

        [TestMethod]
        public void T03RatingsTest()
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ReplayContext>();
            var comboRatingCalcService = scope.ServiceProvider.GetRequiredService<ComboRatingCalcService>();

            comboRatingCalcService.ProduceRatings(new()
            {
                RatingType = RatingNgType.All,
                GameModes = [GameMode.Standard, GameMode.Commanders, GameMode.CommandersHeroic, GameMode.BrawlCommanders],
                Take = 100_000
            }).Wait();

            var replayNgRatings = context.ReplayNgRatings.ToList();
            var playerNgRatings = context.PlayerNgRatings.ToList();

            Assert.IsTrue(replayNgRatings.Count > 0);
            Assert.IsTrue(replayNgRatings.All(a => !a.IsPreRating));
            Assert.IsTrue(playerNgRatings.Count > 0);
        }
    }
}