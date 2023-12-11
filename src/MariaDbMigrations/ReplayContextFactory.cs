using dsstats.db8;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace MariaDbMigrations;

public class ReplayContextFactory : IDesignTimeDbContextFactory<ReplayContext>
{
    public ReplayContext CreateDbContext(string[] args)
    {
        var json = JsonSerializer.Deserialize<JsonElement>(File.ReadAllText("/data/localserverconfig.json"));
        var config = json.GetProperty("ServerConfig");
        var connectionString = config.GetProperty("MariaDbConnectionString").GetString();
        var serverVersion = new MariaDbServerVersion(new Version(11, 1, 2));

        var optionsBuilder = new DbContextOptionsBuilder<ReplayContext>();
        optionsBuilder.UseMySql(connectionString, serverVersion, x =>
        {
            x.EnableRetryOnFailure();
            x.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
            x.MigrationsAssembly("MariaDbMigrations");
        });

        return new ReplayContext(optionsBuilder.Options);
    }
}
