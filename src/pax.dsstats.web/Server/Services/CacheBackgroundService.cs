﻿using pax.dsstats.dbng.Repositories;
using pax.dsstats.dbng.Services;
using System.Diagnostics;

namespace pax.dsstats.web.Server.Services;



public class CacheBackgroundService : IHostedService, IDisposable
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<CacheBackgroundService> logger;
    private Timer? _timer;
    private SemaphoreSlim ss = new(1, 1);

    public CacheBackgroundService(IServiceProvider serviceProvider, ILogger<CacheBackgroundService> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _timer = new Timer(DoWork, null, new TimeSpan(0, 4, 0), new TimeSpan(1, 0, 0));
        // _timer = new Timer(DoWork, null, new TimeSpan(0, 0, 4), new TimeSpan(0, 1, 0));
        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        await ss.WaitAsync();
        try
        {
            using var scope = serviceProvider.CreateScope();
            var importService = scope.ServiceProvider.GetRequiredService<ImportService>();

            Stopwatch sw = Stopwatch.StartNew();

            var result = await importService.ImportReplayBlobs();
            if (result.BlobFiles > 0)
            {
                logger.LogWarning(result.ToString());
            }

            if (result.SavedReplays > 0)
            {
                var statsService = scope.ServiceProvider.GetRequiredService<IStatsService>();
                statsService.ResetStatsCache();
                await statsService.GetRequestStats(new shared.StatsRequest() { Uploaders = false });

                var mmrService = scope.ServiceProvider.GetRequiredService<MmrService>();
                await mmrService.ReCalculateWithDictionary(DateTime.MinValue, DateTime.Today.AddDays(1));
            }

            var replayRepository = scope.ServiceProvider.GetRequiredService<IReplayRepository>();
            await replayRepository.SetReplayViews();

            sw.Stop();
            logger.LogWarning($"{DateTime.UtcNow.ToString(@"yyyy-MM-dd HH:mm:ss")} - Work done in {sw.ElapsedMilliseconds} ms");
        }
        catch (Exception ex)
        {
            logger.LogError($"job failed: {ex.Message}");
        }
        finally
        {
            ss.Release();
        }
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}

