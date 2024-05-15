﻿
using dsstats.db8services;
using dsstats.shared;
using dsstats.shared.Interfaces;
using System.Collections.Concurrent;

namespace dsstats.api.Services;

public partial class IhService(IServiceScopeFactory scopeFactory) : IIhService
{
    private ConcurrentDictionary<Guid, GroupState> groups = [];
    private ConcurrentDictionary<Guid, List<IhReplay>> groupReplays = [];
    SemaphoreSlim decodeSS = new(1, 1);
    SemaphoreSlim playerSS = new(1, 1);

    public async Task<List<GroupStateDto>> GetOpenGroups()
    {
        using var scope = scopeFactory.CreateScope();
        var ihRepository = scope.ServiceProvider.GetRequiredService<IIhRepository>();
        return await ihRepository.GetOpenGroups();
    }

    public async Task<GroupState> CreateOrVisitGroup(Guid groupId)
    {
        if (groups.TryGetValue(groupId, out GroupState? groupState)
            && groupState is not null)
        {
            groupState.Visitors++;
        }
        else
        {
            using var scope = scopeFactory.CreateScope();
            var ihRepository = scope.ServiceProvider.GetRequiredService<IIhRepository>();
            groupState = await ihRepository.GetOrCreateGroupState(groupId);
            groupState.Visitors++;
            groups.AddOrUpdate(groupId, groupState,
                (k, v) => v = v with { Visitors = groupState.Visitors });
        }

        if (!groupReplays.TryGetValue(groupId, out List<IhReplay>? replays)
            || replays is null)
        {
            groupReplays.AddOrUpdate(groupId, [], (k, v) => v = []);
        }
        return groupState;
    }

    public GroupState? LeaveGroup(Guid groupId)
    {
        if (groups.TryGetValue(groupId, out GroupState? groupState)
            && groupState is not null)
        {
            groupState.Visitors--;
            return groupState;
        }
        return null;
    }

    public async Task<GroupState?> GetDecodeResultAsync(Guid guid)
    {
        List<IhReplay> replays = [];

        if (groups.TryGetValue(guid, out GroupState? groupState)
            && groupState is not null)
        {
            using var scope = scopeFactory.CreateScope();
            var decodeService = scope.ServiceProvider.GetRequiredService<DecodeService>();

            var completionSource = new TaskCompletionSource<List<IhReplay>>();

            EventHandler<DecodeEventArgs>? decodeEventHandler = null;

            decodeEventHandler = (sender, args) =>
            {
                if (args.Guid == guid)
                {
                    decodeService.DecodeFinished -= decodeEventHandler;
                    completionSource.SetResult(args.IhReplays);
                }
            };
            decodeService.DecodeFinished += decodeEventHandler;

            var timeoutTask = Task.Delay(20000);

            var completedTask = await Task.WhenAny(completionSource.Task, timeoutTask);

            if (completedTask == timeoutTask)
            {
                decodeService.DecodeFinished -= decodeEventHandler;
                return null;
            }

            var result = await completionSource.Task;
            await decodeSS.WaitAsync();
            try
            {
                foreach (var replay in result)
                {
                    if (groupState.ReplayHashes.Contains(replay.Replay.ReplayHash))
                    {
                        continue;
                    }
                    groupState.ReplayHashes.Add(replay.Replay.ReplayHash);
                    replays.Add(replay);
                    groupReplays[guid].Add(replay);
                }
                await SetReplayStats(groupState, replays);
                var ihRepository = scope.ServiceProvider.GetRequiredService<IIhRepository>();
                // await ihRepository.CalcultePerformance(groupState);
                await ihRepository.UpdateGroupState(groupState);
            }
            finally
            {
                decodeSS.Release();
            }
        }
        return groupState;
    }

    public async Task<PlayerState?> AddPlayerToGroup(Guid groupId, RequestNames requestNames)
    {
        await playerSS.WaitAsync();
        try
        {
            if (!groups.TryGetValue(groupId, out GroupState? groupState)
                || groupState is null)
            {
                return null;
            }

            PlayerId playerId = new(requestNames.ToonId, requestNames.RealmId, requestNames.RegionId);
            if (groupState.PlayerStates.Any(a => a.PlayerId == playerId))
            {
                return null;
            }

            (var name, var rating) = await GetNameAndRating(groupState, playerId);

            PlayerState playerState = new()
            {
                PlayerId = playerId,
                Name = requestNames.Name,
                RatingStart = rating,
                InQueue = true,
                QueuePriority = QueuePriority.High
            };
            groupState.PlayerStates.Add(playerState);

            using var scope = scopeFactory.CreateScope();
            var ihRepository = scope.ServiceProvider.GetRequiredService<IIhRepository>();
            await ihRepository.UpdateGroupState(groupState);
            return playerState;
        }
        finally
        {
            playerSS.Release();
        }
    }

    public async Task<PlayerState?> RemovePlayerFromGroup(Guid groupId, RequestNames requestNames)
    {
        await playerSS.WaitAsync();
        try
        {
            if (!groups.TryGetValue(groupId, out GroupState? groupState)
                || groupState is null)
            {
                return null;
            }

            PlayerId playerId = new(requestNames.ToonId, requestNames.RealmId, requestNames.RegionId);
            var playerState = groupState.PlayerStates.FirstOrDefault(f => f.PlayerId == playerId);

            if (playerState is null)
            {
                return null;
            }

            groupState.PlayerStates.Remove(playerState);

            using var scope = scopeFactory.CreateScope();
            var ihRepository = scope.ServiceProvider.GetRequiredService<IIhRepository>();
            await ihRepository.UpdateGroupState(groupState);
            return await Task.FromResult(playerState);
        }
        finally
        {
            playerSS.Release();
        }
    }

    public async Task<bool> AddPlayerToQueue(Guid groupId, PlayerId playerId)
    {
        if (!groups.TryGetValue(groupId, out GroupState? groupState)
            || groupState is null)
        {
            return false;
        }

        var playerState = groupState.PlayerStates.FirstOrDefault(f => f.PlayerId == playerId);

        if (playerState is null)
        {
            return false;
        }

        playerState.InQueue = true;
        return await Task.FromResult(true);
    }

    public async Task<bool> RemovePlayerFromQueue(Guid groupId, PlayerId playerId)
    {
        if (!groups.TryGetValue(groupId, out GroupState? groupState)
            || groupState is null)
        {
            return false;
        }

        var playerState = groupState.PlayerStates.FirstOrDefault(f => f.PlayerId == playerId);

        if (playerState is null)
        {
            return false;
        }

        playerState.InQueue = false;
        return await Task.FromResult(true);
    }

    public async Task<List<ReplayListDto>> GetReplays(Guid groupId)
    {
        using var scope = scopeFactory.CreateScope();
        var ihRepository = scope.ServiceProvider.GetRequiredService<IIhRepository>();
        return await ihRepository.GetReplays(groupId);
    }

    public async Task<GroupState?> CalculatePerformance(Guid guid)
    {
        using var scope = scopeFactory.CreateScope();
        var ihRepository = scope.ServiceProvider.GetRequiredService<IIhRepository>();
        var groupState = await ihRepository.GetOrCreateGroupState(guid);
        await ihRepository.CalcultePerformance(groupState);
        return groupState;
    }

    public async Task Cleanup()
    {
        DateTime bp = DateTime.UtcNow.AddHours(-24);
        var oldGroupIds = groups.Values.Where(x => x.Created < bp).Select(s => s.GroupId).ToList();

        if (oldGroupIds.Count == 0)
        {
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var ihRepository = scope.ServiceProvider.GetRequiredService<IIhRepository>();
        
        foreach (var groupId in oldGroupIds)
        {
            groups.TryRemove(groupId, out _);
            groupReplays.TryRemove(groupId, out _);
            await ihRepository.CloseGroup(groupId);
        }
    }
}

