using dsstats.shared.Interfaces;
using Microsoft.Extensions.DependencyInjection;
namespace dsstats.db8services;

public static class StatsServiceCollectionExtensions
{
    public static IServiceCollection AddStats(this IServiceCollection services)
    {
        services.AddScoped<IWinrateNgService, WinrateNgService>();
        return services;
    }
}

