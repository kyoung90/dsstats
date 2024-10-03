using dsstats.db8.Ratings;
using dsstats.shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;

namespace dsstats.db8;

public class ReplayContext : DbContext
{
    public DbSet<Uploader> Uploaders { get; set; }
    public DbSet<BattleNetInfo> BattleNetInfos { get; set; }
    public DbSet<Player> Players { get; set; }
    public DbSet<NoUploadResult> NoUploadResults { get; set; }
    public DbSet<PlayerRating> PlayerRatings { get; set; }
    public DbSet<PlayerRatingChange> PlayerRatingChanges { get; set; }
    public DbSet<Replay> Replays { get; set; }
    public DbSet<ReplayPlayer> ReplayPlayers { get; set; }
    public DbSet<ReplayRating> ReplayRatings { get; set; }
    public DbSet<RepPlayerRating> RepPlayerRatings { get; set; }
    public DbSet<PlayerUpgrade> PlayerUpgrades { get; set; }
    public DbSet<Spawn> Spawns { get; set; }
    public DbSet<SpawnUnit> SpawnUnits { get; set; }
    public DbSet<Unit> Units { get; set; }
    public DbSet<Upgrade> Upgrades { get; set; }
    public DbSet<ReplayEvent> ReplayEvents { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<ReplayViewCount> ReplayViewCounts { get; set; }
    public DbSet<ReplayDownloadCount> ReplayDownloadCounts { get; set; }
    public DbSet<SkipReplay> SkipReplays { get; set; }
    public DbSet<CommanderMmr> CommanderMmrs { get; set; }
    public DbSet<GroupByHelper> GroupByHelpers { get; set; }
    public DbSet<FunStatsMemory> FunStatMemories { get; set; }
    public DbSet<ArcadeReplay> ArcadeReplays { get; set; }
    public DbSet<MaterializedArcadeReplay> MaterializedArcadeReplays { get; set; }
    public DbSet<ArcadeReplayDsPlayer> ArcadeReplayDsPlayers { get; set; }
    public DbSet<ArcadeReplayRating> ArcadeReplayRatings { get; set; }
    public DbSet<ArcadePlayerRating> ArcadePlayerRatings { get; set; }
    public DbSet<ArcadeReplayDsPlayerRating> ArcadeReplayDsPlayerRatings { get; set; }
    public DbSet<ArcadePlayerRatingChange> ArcadePlayerRatingChanges { get; set; }
    public DbSet<DsUpdate> DsUpdates { get; set; }
    public DbSet<DsUnit> DsUnits { get; set; }
    public DbSet<DsWeapon> DsWeapons { get; set; }
    public DbSet<BonusDamage> BonusDamages { get; set; }
    public DbSet<DsAbility> DsAbilities { get; set; }
    public DbSet<DsUpgrade> DsUpgrades { get; set; }
    public DbSet<ReplayArcadeMatch> ReplayArcadeMatches { get; set; }

    public DbSet<ComboPlayerRating> ComboPlayerRatings { get; set; }
    public DbSet<ComboReplayRating> ComboReplayRatings { get; set; }
    public DbSet<ComboReplayPlayerRating> ComboReplayPlayerRatings { get; set; }
    public DbSet<Faq> Faqs { get; set; }
    public DbSet<FaqVote> FaqVotes { get; set; }
    public DbSet<IhSession> IhSessions { get; set; }
    public DbSet<IhSessionPlayer> IhSessionPlayers { get; set; }
    public DbSet<DsPickBan> DsPickBans { get; set; }
    public DbSet<PlayerDsRating> PlayerDsRatings { get; set; }
    public DbSet<ReplayPlayerDsRating> ReplayPlayerDsRatings { get; set; }
    public DbSet<ReplayDsRating> ReplayDsRatings { get; set; }
    public int Week(DateTime date) => throw new InvalidOperationException($"{nameof(Week)} cannot be called client side.");
    public int Strftime(string arg, DateTime date) => throw new InvalidOperationException($"{nameof(Strftime)} cannot be called client side.");

    public virtual DbSet<StreakInfo> StreakInfos { get; set; }

    public ReplayContext(DbContextOptions<ReplayContext> options)
    : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StreakInfo>(entity =>
        {
            entity.HasNoKey();
        });

        modelBuilder.Entity<Replay>(entity =>
        {
            entity.HasIndex(e => e.FileName);
            entity.HasIndex(e => e.Maxkillsum);
            entity.HasIndex(e => new { e.GameTime });
            entity.HasIndex(e => new { e.GameTime, e.GameMode });
            entity.HasIndex(e => new { e.GameTime, e.GameMode, e.DefaultFilter });
            entity.HasIndex(e => new { e.GameTime, e.GameMode, e.WinnerTeam });
            entity.HasIndex(e => new { e.GameTime, e.GameMode, e.Maxleaver });
            entity.HasIndex(e => e.Imported);

            entity.Property(p => p.ReplayHash)
                .HasMaxLength(64)
                .IsFixedLength();

            entity.HasIndex(e => e.ReplayHash)
                .IsUnique();
        });

        modelBuilder.Entity<ReplayPlayer>(entity =>
        {
            entity.HasIndex(e => e.Race);
            entity.HasIndex(e => new { e.Race, e.OppRace });
            entity.HasIndex(e => e.Kills);
            entity.HasIndex(e => new { e.IsUploader, e.Team });
            entity.HasIndex(e => e.Name);

            entity.Property(p => p.LastSpawnHash)
                .HasMaxLength(64)
                .IsFixedLength();
            entity.HasIndex(e => e.LastSpawnHash)
                .IsUnique();
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasIndex(e => new { e.RegionId, e.RealmId, e.ToonId }).IsUnique();
        });

        modelBuilder.Entity<Uploader>(entity =>
        {
            entity.HasIndex(e => e.AppGuid).IsUnique();
        });

        modelBuilder.Entity<Uploader>()
            .HasMany(p => p.Replays)
            .WithMany(p => p.Uploaders)
            .UsingEntity(j => j.ToTable("UploaderReplays"));

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Upgrade>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<CommanderMmr>(entity =>
        {
            entity.HasIndex(e => new { e.Race, e.OppRace });
        });

        modelBuilder.Entity<GroupByHelper>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("GroupByHelper");
            entity.Property(p => p.Group).HasColumnName("Name");
        });

        modelBuilder.Entity<ComboPlayerRating>(entity =>
        {
            entity.HasIndex(i => i.RatingType);
        });

        modelBuilder.Entity<ComboReplayRating>(entity =>
        {
            entity.HasIndex(i => i.RatingType);
        });

        modelBuilder.Entity<PlayerRating>(entity =>
        {
            entity.HasIndex(e => e.RatingType);
        });

        modelBuilder.Entity<ArcadeReplay>(entity =>
        {
            entity.HasIndex(i => new { i.GameMode, i.CreatedAt });
            entity.HasIndex(i => new { i.RegionId, i.GameMode, i.CreatedAt });
            entity.HasIndex(i => new { i.RegionId, i.BnetBucketId, i.BnetRecordId }).IsUnique();
            entity.HasIndex(i => i.ReplayHash);
        });

        modelBuilder.Entity<ArcadePlayerRating>(entity =>
        {
            entity.HasIndex(i => i.RatingType);
        });

        modelBuilder.Entity<ReplayRating>(entity =>
        {
            entity.HasIndex(i => i.RatingType);
        });

        modelBuilder.Entity<DsUpdate>(entity =>
        {
            entity.HasIndex(i => i.Time);
        });

        modelBuilder.Entity<DsUnit>(entity =>
        {
            entity.HasIndex(i => i.Name);
            entity.HasIndex(i => i.Commander);
            entity.HasIndex(i => new { i.Name, i.Commander });
        });

        modelBuilder.Entity<BonusDamage>(entity =>
        {
            entity.HasIndex(i => i.UnitType);
        });

        modelBuilder.Entity<DsAbility>(entity =>
        {
            entity.HasIndex(i => i.Name);
        });

        modelBuilder.Entity<DsUpgrade>(entity =>
        {
            entity.HasIndex(i => i.Upgrade);
        });

        modelBuilder.Entity<Faq>(entity =>
        {
            entity.HasIndex(i => i.Question);
        });

        modelBuilder.Entity<IhSession>(entity =>
        {
            entity.HasIndex(i => i.GroupId).IsUnique();
            entity.Property(p => p.GroupState).HasConversion(
                c => JsonSerializer.Serialize(c, (JsonSerializerOptions?)null),
                c => JsonSerializer.Deserialize<GroupState>(c, (JsonSerializerOptions?)null));
            entity.Property(p => p.GroupStateV2).HasConversion(
                c => JsonSerializer.Serialize(c, (JsonSerializerOptions?)null),
                c => JsonSerializer.Deserialize<GroupStateV2>(c, (JsonSerializerOptions?)null));
        });

        modelBuilder.Entity<ReplayArcadeMatch>(entity =>
        {
            entity.HasIndex(i => i.ReplayId).IsUnique();
            entity.HasIndex(i => i.ArcadeReplayId).IsUnique();
            entity.HasIndex(i => i.MatchTime);
        });

        modelBuilder.Entity<MaterializedArcadeReplay>(entity =>
        {
            entity.HasIndex(i => i.CreatedAt);
        });

        modelBuilder.Entity<PlayerDsRating>(entity =>
        {
            entity.HasIndex(i => new { i.PlayerId, i.RatingType }).IsUnique();
        });

        MethodInfo weekMethodInfo = typeof(ReplayContext)
            .GetRuntimeMethod(nameof(ReplayContext.Week), new[] { typeof(DateTime) }) ?? throw new ArgumentNullException();

        modelBuilder.HasDbFunction(weekMethodInfo)
           .HasTranslation(args =>
                    new SqlFunctionExpression("WEEK",
                        new[]
                        {
                            args.ToArray()[0],
                            new SqlConstantExpression(Expression.Constant(3, typeof(int)), new IntTypeMapping("int")),
                        },
                        true,
                        new[] { false, false },
                        typeof(int),
                        null
                    )
                );

        MethodInfo strftimeMethodInfo = typeof(ReplayContext)
            .GetRuntimeMethod(nameof(ReplayContext.Strftime), new[] { typeof(string), typeof(DateTime) }) ?? throw new ArgumentNullException();

        modelBuilder.HasDbFunction(strftimeMethodInfo)
           .HasTranslation(args =>
                    new SqlFunctionExpression("strftime",
                        new[]
                        {
                            new SqlFragmentExpression((args.ToArray()[0] as SqlConstantExpression)?.Value?.ToString() ?? string.Empty),
                            args.ToArray()[1]
                        },
                        true,
                        new[] { false, false },
                        typeof(int),
                        null
                    )
                );
    }
}

public record StreakInfo
{
    public int PlayerResult { get; set; }
    public double LongestStreak { get; set; }
}