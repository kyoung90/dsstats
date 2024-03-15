using dsstats.ratings;
using dsstats.shared;
using Microsoft.Extensions.DependencyInjection;

namespace dsstats.db8services.Import;

public partial class ImportService
{
    public async Task SetPreRatings()
    {
        if (IsMaui)
        {
            return;
        }

        using var scope = serviceProvider.CreateScope();
        var comboRatingCalcServices = scope.ServiceProvider.GetRequiredService<ComboRatingCalcService>();

        await comboRatingCalcServices.ProducePreRatings(new CalcRequest()
        {
            FromDate = DateTime.UtcNow.AddHours(-1),
            RatingType = RatingNgType.All,
            GameModes = [GameMode.Standard, GameMode.Commanders, GameMode.CommandersHeroic, GameMode.BrawlCommanders],
            Take = 100_000
        });
    }
}
