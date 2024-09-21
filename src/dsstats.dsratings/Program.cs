using dsstats.db8.AutoMapper;
using dsstats.db8;
using dsstats.shared;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace dsstats.dsratings;

internal class Program
{
    static void Main(string[] args)
    {
        var services = new ServiceCollection();

        var jsonStrg = File.ReadAllText("/data/localserverconfig.json");
        var json = JsonSerializer.Deserialize<JsonElement>(jsonStrg);
        var config = json.GetProperty("ServerConfig");
        var importConnectionString = config.GetProperty("Import8ConnectionString").GetString() ?? "";
        var mySqlConnectionString = config.GetProperty("Dsstats8ConnectionString").GetString();
        // var mySqlConnectionString = config.GetProperty("ProdConnectionString").GetString();

        services.AddOptions<DbImportOptions>()
            .Configure(x =>
            {
                x.ImportConnectionString = importConnectionString;
                x.IsSqlite = false;
            });

        services.AddLogging(options =>
        {
            options.SetMinimumLevel(LogLevel.Information);
            options.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);
            options.AddConsole();
        });

        services.AddDbContext<ReplayContext>(options =>
        {
            options.UseMySql(mySqlConnectionString, ServerVersion.AutoDetect(mySqlConnectionString), p =>
            {
                p.CommandTimeout(600);
                p.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
            });
        });

        services.AddAutoMapper(typeof(AutoMapperProfile));
        services.AddScoped<DsstatsRatings>();

        var serviceProvider = services.BuildServiceProvider();

        var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        Stopwatch sw = Stopwatch.StartNew();
        logger.LogInformation("ratings start.");

        var dsstatsRatings = scope.ServiceProvider.GetRequiredService<DsstatsRatings>();
        dsstatsRatings.CalculateRatings().Wait();

        sw.Stop();
        logger.LogInformation("ratings done. {elapsed}min", sw.Elapsed.TotalMinutes);
    }
}
