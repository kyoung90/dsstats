using Blazored.Toast.Services;
using dsstats.maui.Services;
using dsstats.shared.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using System.Collections.Concurrent;

namespace dsstats.maui.Shared;

public partial class TopRowComponent : ComponentBase, IDisposable
{
    [Inject]
    public DsstatsService dsstatsService { get; set; } = default!;
    [Inject]
    public IRemoteToggleService remoteToggleService { get; set; } = default!;
    [Inject]
    public NavigationManager NavigationManager { get; set; } = default!;
    [Inject]
    public ConfigService configService { get; set; } = default!;
    [Inject]
    public IToastService toastService { get; set; } = default!;

    string currentLocation = "Home";
    DecodeInfoEventArgs? decodeInfo = null;
    ConcurrentBag<DecodeError> decodeErrors = new();
    List<DecodeError> decodeErrorsList = new();
    DecodeErrorModal? decodeErrorModal;

    protected override void OnInitialized()
    {
        dsstatsService.ScanStateChanged += DssstatsService_ScanStateChanged;
        dsstatsService.DecodeStateChanged += DssstatsService_DecodeStateChanged;
        NavigationManager.LocationChanged += NavigationManager_LocationChanged;

        // DEBUG
        decodeErrors.Add(new()
        {
            ReplayPath = "TestPath",
            Error = "TestError"
        });

        base.OnInitialized();
    }

    private void DssstatsService_DecodeStateChanged(object? sender, DecodeInfoEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Info))
        {
            toastService.ShowInfo(e.Info);
        }
        if (e.DecodeError is not null)
        {
            decodeErrors.Add(e.DecodeError);
        }
        decodeInfo = e;
        InvokeAsync(() => StateHasChanged());
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            _ = InitScan();
        }
        base.OnAfterRender(firstRender);
    }

    private async Task InitScan()
    {
        await Task.Delay(1000);
        await dsstatsService.ScanForNewReplays();
    }

    private void DssstatsService_ScanStateChanged(object? sender, ScanEventArgs e)
    {
        InvokeAsync(() => StateHasChanged());
    }

    private void NavigationManager_LocationChanged(object? sender, LocationChangedEventArgs e)
    {
        currentLocation = e.Location.Replace("https://0.0.0.0/", "");
        InvokeAsync(() => StateHasChanged());
    }

    private void ShowErrors()
    {
        decodeErrorsList = decodeErrors.ToList();
        decodeErrorModal?.Show();
    }

    public void Dispose()
    {
        dsstatsService.ScanStateChanged -= DssstatsService_ScanStateChanged;
        dsstatsService.DecodeStateChanged -= DssstatsService_DecodeStateChanged;
        NavigationManager.LocationChanged -= NavigationManager_LocationChanged;
    }

    public record DecodeState
    {
        public int DoneDecoding {  get; set; }
        public TimeSpan Eta { get; set; }
        public TimeSpan Elapsed { get; set; }
        public int Per { get; set; }
        public bool Finished {  get; set; }
    }
}