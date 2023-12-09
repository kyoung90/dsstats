using MysqlMigrations;

namespace Mysql8Migrations;

public static class Migrate5_7To8_0
{
    public static void Migrate()
    {
        var replayContextFactory = new ReplayContextFactory();

        var v8context = replayContextFactory.CreateDbContext([]);
        var v5context = replayContextFactory.CreateDbContextV5_7([]);


    }
}
