﻿using dsstats.db8services;
using dsstats.maui.Services;
using dsstats.shared;
using Microsoft.AspNetCore.Components;

namespace dsstats.maui.Pages;

public partial class Index : ComponentBase, IDisposable
{
    [Inject]
    public IReplayRepository replayRepository { get; set; } = default!;
    [Inject]
    public ConfigService configService { get; set; } = default!;
    [Inject]
    public DsstatsService dsstatsService { get; set; } = default!;

    ReplayDto? currentReplay = null;
    PlayerId? interestPlayer = null;
    bool isLatestreplay = true;

    bool DEBUG = true;

    protected override void OnInitialized()
    {
        _ = LoadLatestReplay();
        dsstatsService.DecodeStateChanged += DssstatsService_DecodeStateChanged;
        base.OnInitialized();
    }

    private void DssstatsService_DecodeStateChanged(object? sender, DecodeInfoEventArgs e)
    {
        if (e.Finished)
        {
            _ = LoadLatestReplay();
        }
        InvokeAsync(() => StateHasChanged());
    }

    private async Task LoadLatestReplay()
    {
        currentReplay = await replayRepository.GetLatestReplay();

        if (currentReplay is null)
        {
            return;
        }

        isLatestreplay = true;

        var appPlayers = configService.GetRequestNames()
            .Select(s => new PlayerId(s.ToonId, s.RealmId, s.RegionId))
            .ToList();

        var repPlayers = currentReplay.ReplayPlayers
            .Select(s => new PlayerId(s.Player.ToonId, s.Player.RealmId, s.Player.RegionId))
            .ToList();

        interestPlayer = repPlayers.FirstOrDefault(f => appPlayers.Contains(f));

        await InvokeAsync(() => StateHasChanged());
    }

    private async Task LoadNextReplay(bool next)
    {
        if (currentReplay is null)
        {
            return;
        }

        if (next)
        {
            var nextReplay = await replayRepository
                .GetNextReplay(currentReplay.GameTime);
            if (nextReplay is null)
            {
                isLatestreplay = true;
                return;
            }
            currentReplay = nextReplay;
        }
        else
        {
            var prevReplay = await replayRepository
                .GetPreviousReplay(currentReplay.GameTime);
            if (prevReplay is null)
            {
                return;
            }
            currentReplay = prevReplay;
        }
        isLatestreplay = false;
        await InvokeAsync(() => StateHasChanged());
    }

    public void Dispose()
    {
        dsstatsService.DecodeStateChanged -= DssstatsService_DecodeStateChanged;
    }
}
