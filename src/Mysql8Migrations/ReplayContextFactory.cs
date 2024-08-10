using dsstats.db8;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace Mysql8Migrations;

public class ReplayContextFactory : IDesignTimeDbContextFactory<ReplayContext>
{
    public ReplayContext CreateDbContext(string[] args)
    {
        var json = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText("/data/localserverconfig.json"));
        var config = json.GetProperty("ServerConfig");
        var connectionString = config.GetProperty("Dsstats8ConnectionString").GetString();
        // var connectionString = config.GetProperty("Prod8ConnectionString").GetString();
        var serverVersion = new MySqlServerVersion(new System.Version(8, 4, 1));

        var optionsBuilder = new DbContextOptionsBuilder<ReplayContext>();
        optionsBuilder.UseMySql(connectionString, serverVersion, x =>
        {
            x.EnableRetryOnFailure();
            x.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
            x.MigrationsAssembly("Mysql8Migrations");
        });

        return new ReplayContext(optionsBuilder.Options);
    }
}