
namespace dsstats.shared.Interfaces;

public interface IWinrateService
{
    Task<WinrateResponse> GetWinrate(WinrateRequest request, CancellationToken token);
    Task<WinrateResponse> GetWinrate(StatsRequest request, CancellationToken token);
}

public interface IWinrateNgService
{
    Task<WinrateResponse> GetWinrate(WinrateNgRequest request, CancellationToken token);
}