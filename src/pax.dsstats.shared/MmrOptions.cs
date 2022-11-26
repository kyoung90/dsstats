﻿namespace pax.dsstats.shared;

public record MmrOptions
{
    public bool Continue { get; set; }
    public bool UseCommanderMmr { get; set; } = false;
    public bool UseConsistency { get; set; } = true;
    public bool UseFactorToTeamMates { get; set; } = false;
    public bool UseConfidence { get; set; } = true;
}