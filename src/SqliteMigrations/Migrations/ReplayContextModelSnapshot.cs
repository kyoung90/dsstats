﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using pax.dsstats.dbng;

#nullable disable

namespace SqliteMigrations.Migrations
{
    [DbContext(typeof(ReplayContext))]
    partial class ReplayContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.10");

            modelBuilder.Entity("pax.dsstats.dbng.BattleNetInfo", b =>
                {
                    b.Property<int>("BattleNetInfoId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BattleNetId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UploaderId")
                        .HasColumnType("INTEGER");

                    b.HasKey("BattleNetInfoId");

                    b.HasIndex("UploaderId");

                    b.ToTable("BattleNetInfos");
                });

            modelBuilder.Entity("pax.dsstats.dbng.CommanderMmr", b =>
                {
                    b.Property<int>("CommanderMmrId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("AntiSynergyElo_1")
                        .HasColumnType("REAL");

                    b.Property<double>("AntiSynergyElo_2")
                        .HasColumnType("REAL");

                    b.Property<double>("AntiSynergyMmr_1")
                        .HasColumnType("REAL");

                    b.Property<double>("AntiSynergyMmr_2")
                        .HasColumnType("REAL");

                    b.Property<int>("Commander_1")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Commander_2")
                        .HasColumnType("INTEGER");

                    b.Property<double>("SynergyMmr")
                        .HasColumnType("REAL");

                    b.HasKey("CommanderMmrId");

                    b.HasIndex("Commander_1", "Commander_2");

                    b.ToTable("CommanderMmrs");
                });

            modelBuilder.Entity("pax.dsstats.dbng.Event", b =>
                {
                    b.Property<int>("EventId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("EventGuid")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("EventStart")
                        .HasPrecision(0)
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.HasKey("EventId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Events");
                });

            modelBuilder.Entity("pax.dsstats.dbng.GroupByHelper", b =>
                {
                    b.Property<int>("Count")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Group")
                        .HasColumnType("INTEGER")
                        .HasColumnName("Name");

                    b.ToView("GroupByHelper");
                });

            modelBuilder.Entity("pax.dsstats.dbng.Player", b =>
                {
                    b.Property<int>("PlayerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("GamesCmdr")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GamesStd")
                        .HasColumnType("INTEGER");

                    b.Property<int>("LeaverCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MainCommander")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MainCount")
                        .HasColumnType("INTEGER");

                    b.Property<double>("Mmr")
                        .HasColumnType("REAL");

                    b.Property<string>("MmrOverTime")
                        .HasMaxLength(2000)
                        .HasColumnType("TEXT");

                    b.Property<double>("MmrStd")
                        .HasColumnType("REAL");

                    b.Property<string>("MmrStdOverTime")
                        .HasMaxLength(2000)
                        .HasColumnType("TEXT");

                    b.Property<int>("MvpCmdr")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MvpStd")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int>("NotUploadCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("RegionId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TeamGamesCmdr")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TeamGamesStd")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ToonId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("UploaderId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WinsCmdr")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WinsStd")
                        .HasColumnType("INTEGER");

                    b.HasKey("PlayerId");

                    b.HasIndex("ToonId")
                        .IsUnique();

                    b.HasIndex("UploaderId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("pax.dsstats.dbng.PlayerUpgrade", b =>
                {
                    b.Property<int>("PlayerUpgradeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Gameloop")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ReplayPlayerId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UpgradeId")
                        .HasColumnType("INTEGER");

                    b.HasKey("PlayerUpgradeId");

                    b.HasIndex("ReplayPlayerId");

                    b.HasIndex("UpgradeId");

                    b.ToTable("PlayerUpgrades");
                });

            modelBuilder.Entity("pax.dsstats.dbng.Replay", b =>
                {
                    b.Property<int>("ReplayId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Bunker")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Cannon")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CommandersTeam1")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("CommandersTeam2")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("DefaultFilter")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Downloads")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Duration")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.Property<int>("GameMode")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("GameTime")
                        .HasPrecision(0)
                        .HasColumnType("TEXT");

                    b.Property<int>("Maxkillsum")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Maxleaver")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Middle")
                        .IsRequired()
                        .HasMaxLength(4000)
                        .HasColumnType("TEXT");

                    b.Property<int>("Minarmy")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Minincome")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Minkillsum")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Objective")
                        .HasColumnType("INTEGER");

                    b.Property<byte>("Playercount")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("ReplayEventId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ReplayHash")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT")
                        .IsFixedLength();

                    b.Property<int>("Views")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WinnerTeam")
                        .HasColumnType("INTEGER");

                    b.HasKey("ReplayId");

                    b.HasIndex("FileName");

                    b.HasIndex("Maxkillsum");

                    b.HasIndex("ReplayEventId");

                    b.HasIndex("ReplayHash")
                        .IsUnique();

                    b.HasIndex("GameTime", "GameMode");

                    b.HasIndex("GameTime", "GameMode", "DefaultFilter");

                    b.HasIndex("GameTime", "GameMode", "Maxleaver");

                    b.HasIndex("GameTime", "GameMode", "WinnerTeam");

                    b.ToTable("Replays");
                });

            modelBuilder.Entity("pax.dsstats.dbng.ReplayDownloadCount", b =>
                {
                    b.Property<int>("ReplayDownloadCountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ReplayHash")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.HasKey("ReplayDownloadCountId");

                    b.ToTable("ReplayDownloadCounts");
                });

            modelBuilder.Entity("pax.dsstats.dbng.ReplayEvent", b =>
                {
                    b.Property<int>("ReplayEventId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Ban1")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Ban2")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Ban3")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Ban4")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Ban5")
                        .HasColumnType("INTEGER");

                    b.Property<int>("EventId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Round")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<string>("RunnerTeam")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("WinnerTeam")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("ReplayEventId");

                    b.HasIndex("EventId");

                    b.ToTable("ReplayEvents");
                });

            modelBuilder.Entity("pax.dsstats.dbng.ReplayPlayer", b =>
                {
                    b.Property<int>("ReplayPlayerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("APM")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Army")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Clan")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<bool>("DidNotUpload")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Downloads")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Duration")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GamePos")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Income")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsLeaver")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsUploader")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Kills")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LastSpawnHash")
                        .HasMaxLength(64)
                        .HasColumnType("TEXT")
                        .IsFixedLength();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<int>("OppRace")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PlayerId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PlayerResult")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Race")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Refineries")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("TEXT");

                    b.Property<int>("ReplayId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Team")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TierUpgrades")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("TEXT");

                    b.Property<int?>("UpgradeId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UpgradesSpent")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Views")
                        .HasColumnType("INTEGER");

                    b.HasKey("ReplayPlayerId");

                    b.HasIndex("Kills");

                    b.HasIndex("LastSpawnHash")
                        .IsUnique();

                    b.HasIndex("PlayerId");

                    b.HasIndex("Race");

                    b.HasIndex("ReplayId");

                    b.HasIndex("UpgradeId");

                    b.HasIndex("IsUploader", "Team");

                    b.HasIndex("Race", "OppRace");

                    b.ToTable("ReplayPlayers");
                });

            modelBuilder.Entity("pax.dsstats.dbng.ReplayViewCount", b =>
                {
                    b.Property<int>("ReplayViewCountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ReplayHash")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("TEXT");

                    b.HasKey("ReplayViewCountId");

                    b.ToTable("ReplayViewCounts");
                });

            modelBuilder.Entity("pax.dsstats.dbng.SkipReplay", b =>
                {
                    b.Property<int>("SkipReplayId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Path")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("TEXT");

                    b.HasKey("SkipReplayId");

                    b.ToTable("SkipReplays");
                });

            modelBuilder.Entity("pax.dsstats.dbng.Spawn", b =>
                {
                    b.Property<int>("SpawnId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("ArmyValue")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Breakpoint")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Gameloop")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GasCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Income")
                        .HasColumnType("INTEGER");

                    b.Property<int>("KilledValue")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ReplayPlayerId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UpgradeSpent")
                        .HasColumnType("INTEGER");

                    b.HasKey("SpawnId");

                    b.HasIndex("ReplayPlayerId");

                    b.ToTable("Spawns");
                });

            modelBuilder.Entity("pax.dsstats.dbng.SpawnUnit", b =>
                {
                    b.Property<int>("SpawnUnitId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<byte>("Count")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Poss")
                        .IsRequired()
                        .HasMaxLength(4000)
                        .HasColumnType("TEXT");

                    b.Property<int>("SpawnId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UnitId")
                        .HasColumnType("INTEGER");

                    b.HasKey("SpawnUnitId");

                    b.HasIndex("SpawnId");

                    b.HasIndex("UnitId");

                    b.ToTable("SpawnUnits");
                });

            modelBuilder.Entity("pax.dsstats.dbng.Unit", b =>
                {
                    b.Property<int>("UnitId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Commander")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Cost")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("UnitId");

                    b.HasIndex("Name", "Commander")
                        .IsUnique();

                    b.ToTable("Units");
                });

            modelBuilder.Entity("pax.dsstats.dbng.Upgrade", b =>
                {
                    b.Property<int>("UpgradeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Cost")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("UpgradeId");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.ToTable("Upgrades");
                });

            modelBuilder.Entity("pax.dsstats.dbng.Uploader", b =>
                {
                    b.Property<int>("UploaderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<Guid>("AppGuid")
                        .HasColumnType("TEXT");

                    b.Property<string>("AppVersion")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("Games")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Identifier")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsDeleted")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LatestReplay")
                        .HasPrecision(0)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LatestUpload")
                        .HasPrecision(0)
                        .HasColumnType("TEXT");

                    b.Property<int>("MainCommander")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MainCount")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Mvp")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TeamGames")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UploadDisabledCount")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("UploadIsDisabled")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("UploadLastDisabled")
                        .HasPrecision(0)
                        .HasColumnType("TEXT");

                    b.Property<int>("Wins")
                        .HasColumnType("INTEGER");

                    b.HasKey("UploaderId");

                    b.HasIndex("AppGuid")
                        .IsUnique();

                    b.ToTable("Uploaders");
                });

            modelBuilder.Entity("ReplayUploader", b =>
                {
                    b.Property<int>("ReplaysReplayId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("UploadersUploaderId")
                        .HasColumnType("INTEGER");

                    b.HasKey("ReplaysReplayId", "UploadersUploaderId");

                    b.HasIndex("UploadersUploaderId");

                    b.ToTable("UploaderReplays", (string)null);
                });

            modelBuilder.Entity("pax.dsstats.dbng.BattleNetInfo", b =>
                {
                    b.HasOne("pax.dsstats.dbng.Uploader", "Uploader")
                        .WithMany("BattleNetInfos")
                        .HasForeignKey("UploaderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Uploader");
                });

            modelBuilder.Entity("pax.dsstats.dbng.Player", b =>
                {
                    b.HasOne("pax.dsstats.dbng.Uploader", "Uploader")
                        .WithMany("Players")
                        .HasForeignKey("UploaderId");

                    b.Navigation("Uploader");
                });

            modelBuilder.Entity("pax.dsstats.dbng.PlayerUpgrade", b =>
                {
                    b.HasOne("pax.dsstats.dbng.ReplayPlayer", "ReplayPlayer")
                        .WithMany("Upgrades")
                        .HasForeignKey("ReplayPlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("pax.dsstats.dbng.Upgrade", "Upgrade")
                        .WithMany()
                        .HasForeignKey("UpgradeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ReplayPlayer");

                    b.Navigation("Upgrade");
                });

            modelBuilder.Entity("pax.dsstats.dbng.Replay", b =>
                {
                    b.HasOne("pax.dsstats.dbng.ReplayEvent", "ReplayEvent")
                        .WithMany("Replays")
                        .HasForeignKey("ReplayEventId");

                    b.Navigation("ReplayEvent");
                });

            modelBuilder.Entity("pax.dsstats.dbng.ReplayEvent", b =>
                {
                    b.HasOne("pax.dsstats.dbng.Event", "Event")
                        .WithMany("ReplayEvents")
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Event");
                });

            modelBuilder.Entity("pax.dsstats.dbng.ReplayPlayer", b =>
                {
                    b.HasOne("pax.dsstats.dbng.Player", "Player")
                        .WithMany("ReplayPlayers")
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("pax.dsstats.dbng.Replay", "Replay")
                        .WithMany("ReplayPlayers")
                        .HasForeignKey("ReplayId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("pax.dsstats.dbng.Upgrade", null)
                        .WithMany("ReplayPlayers")
                        .HasForeignKey("UpgradeId");

                    b.Navigation("Player");

                    b.Navigation("Replay");
                });

            modelBuilder.Entity("pax.dsstats.dbng.Spawn", b =>
                {
                    b.HasOne("pax.dsstats.dbng.ReplayPlayer", "ReplayPlayer")
                        .WithMany("Spawns")
                        .HasForeignKey("ReplayPlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("ReplayPlayer");
                });

            modelBuilder.Entity("pax.dsstats.dbng.SpawnUnit", b =>
                {
                    b.HasOne("pax.dsstats.dbng.Spawn", "Spawn")
                        .WithMany("Units")
                        .HasForeignKey("SpawnId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("pax.dsstats.dbng.Unit", "Unit")
                        .WithMany()
                        .HasForeignKey("UnitId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Spawn");

                    b.Navigation("Unit");
                });

            modelBuilder.Entity("ReplayUploader", b =>
                {
                    b.HasOne("pax.dsstats.dbng.Replay", null)
                        .WithMany()
                        .HasForeignKey("ReplaysReplayId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("pax.dsstats.dbng.Uploader", null)
                        .WithMany()
                        .HasForeignKey("UploadersUploaderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("pax.dsstats.dbng.Event", b =>
                {
                    b.Navigation("ReplayEvents");
                });

            modelBuilder.Entity("pax.dsstats.dbng.Player", b =>
                {
                    b.Navigation("ReplayPlayers");
                });

            modelBuilder.Entity("pax.dsstats.dbng.Replay", b =>
                {
                    b.Navigation("ReplayPlayers");
                });

            modelBuilder.Entity("pax.dsstats.dbng.ReplayEvent", b =>
                {
                    b.Navigation("Replays");
                });

            modelBuilder.Entity("pax.dsstats.dbng.ReplayPlayer", b =>
                {
                    b.Navigation("Spawns");

                    b.Navigation("Upgrades");
                });

            modelBuilder.Entity("pax.dsstats.dbng.Spawn", b =>
                {
                    b.Navigation("Units");
                });

            modelBuilder.Entity("pax.dsstats.dbng.Upgrade", b =>
                {
                    b.Navigation("ReplayPlayers");
                });

            modelBuilder.Entity("pax.dsstats.dbng.Uploader", b =>
                {
                    b.Navigation("BattleNetInfos");

                    b.Navigation("Players");
                });
#pragma warning restore 612, 618
        }
    }
}
