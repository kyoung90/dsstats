using dsstats.shared;

namespace dsstats.parser;

public partial class Parser
{
    private static bool FilterUpgrades(string upgradeName, Commander cmdr)
    {
        if (upgradeName.Equals("MineralIncomeBonus", StringComparison.Ordinal)) return true;
        if (upgradeName.StartsWith("AFKTimer", StringComparison.Ordinal)) return true;
        if (upgradeName.Equals("HighCapacityMode", StringComparison.Ordinal)) return true;
        if (upgradeName.Equals("HornerMySignificantOtherBuffHan", StringComparison.Ordinal)) return true;
        if (upgradeName.Equals("HornerMySignificantOtherBuffHorner", StringComparison.Ordinal)) return true;
        if (upgradeName.Equals("StagingAreaNextSpawn", StringComparison.Ordinal)) return true;
        if (upgradeName.StartsWith("Decoration", StringComparison.Ordinal)) return true;
        if (upgradeName.StartsWith("Mastery", StringComparison.Ordinal)) return true;
        if (upgradeName.Equals("MineralIncome", StringComparison.Ordinal)) return true;
        if (upgradeName.StartsWith("Emote", StringComparison.Ordinal)) return true;
        if (upgradeName.Equals("SpookySkeletonNerf", StringComparison.Ordinal)) return true;
        if (upgradeName.Equals("NeosteelFrame", StringComparison.Ordinal)) return true;
        if (upgradeName.EndsWith("Disable")) return true;
        if (upgradeName.EndsWith("Enable")) return true;
        if (upgradeName.StartsWith("Tier", StringComparison.Ordinal)) return true;
        if (upgradeName.EndsWith("Starlight")) return true;
        if (upgradeName.Contains("Worker")) return true;
        if (upgradeName.Equals("PlayerIsAFK", StringComparison.Ordinal)) return true;
        if (upgradeName.Equals("DehakaHeroLevel", StringComparison.Ordinal)) return true;
        if (upgradeName.Equals("DehakaSkillPoint", StringComparison.Ordinal)) return true;
        if (upgradeName.Equals("DehakaHeroPlaceUsed", StringComparison.Ordinal)) return true;
        if (upgradeName.StartsWith("DehakaCreeperHost", StringComparison.Ordinal)) return true;
        if (upgradeName.Contains("PlaceEvolved")) return true;
        if (upgradeName.Equals("KerriganMutatingCarapaceBonus", StringComparison.Ordinal)) return true;
        if (upgradeName.EndsWith("Modification")) return true;
        if (upgradeName.StartsWith("Blacklist", StringComparison.Ordinal)) return true;
        if (upgradeName.Equals("TychusTychusPlaced", StringComparison.Ordinal)) return true;
        if (upgradeName.Equals("TychusFirstOnesontheHouse", StringComparison.Ordinal)) return true;
        if (upgradeName.Equals("ClolarionInterdictorsBonus", StringComparison.Ordinal)) return true;
        if (upgradeName.Equals("PartyFrameHide", StringComparison.Ordinal)) return true;
        if (upgradeName.EndsWith("Bonus")) return true;
        if (upgradeName.Equals("FenixUnlock", StringComparison.Ordinal)) return true;
        if (upgradeName.Equals("FenixExperienceAwarded", StringComparison.Ordinal)) return true;
        if (upgradeName.StartsWith("RaynorCostReduced", StringComparison.Ordinal)) return true;
        if (upgradeName.StartsWith("Theme", StringComparison.Ordinal)) return true;
        if (upgradeName.StartsWith("Worker", StringComparison.Ordinal)) return true;
        if (upgradeName.StartsWith("AreaFlair", StringComparison.Ordinal)) return true;
        if (upgradeName.StartsWith("AreaWeather", StringComparison.Ordinal)) return true;
        if (upgradeName.StartsWith("Aura", StringComparison.Ordinal)) return true;
        if (upgradeName.Equals("HideWorkerCommandCard", StringComparison.Ordinal)) return true;
        if (upgradeName.Equals("UsingVespeneIncapableWorker", StringComparison.Ordinal)) return true;
        if (upgradeName.StartsWith("PowerField", StringComparison.Ordinal)) return true;
        if (upgradeName.EndsWith("Bonus10")) return true;
        if (upgradeName.Equals("DehakaPrimalWurm", StringComparison.Ordinal)) return true;

        return FilterUpgradeLevel(cmdr, upgradeName);
    }

    private static bool FilterUpgradeLevel(Commander cmdr, string upgradename)
    {
        if (upgradename.Contains("Level", StringComparison.Ordinal))
        {
            var urace = cmdr switch
            {
                Commander.Zagara => "Zerg",
                Commander.Abathur => "Zerg",
                Commander.Kerrigan => "Zerg",
                Commander.Alarak => "Protoss",
                Commander.Artanis => "Protoss",
                Commander.Vorazun => "Protoss",
                Commander.Fenix => "Protoss",
                Commander.Karax => "Protoss",
                Commander.Zeratul => "Protoss",
                Commander.Raynor => "Terran",
                Commander.Swann => "Terran",
                Commander.Nova => "Terran",
                Commander.Stukov => "Terran",
                _ => cmdr.ToString()
            };

            if (!upgradename.StartsWith(urace, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
