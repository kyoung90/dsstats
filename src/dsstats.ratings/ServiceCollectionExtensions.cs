using dsstats.ratings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
namespace dsstats.ratings;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRatings(this IServiceCollection services)
    {
        services.AddScoped<DsstatsRatingCalcService>();

        return services;
    }
}

