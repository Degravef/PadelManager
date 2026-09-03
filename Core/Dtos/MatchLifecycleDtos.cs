namespace Core.Dtos;

// Summary of one J-1 daily batch run (RG-ETA-002/003).
public record TraitementQuotidienResultDto(
    DateOnly DateTraitee,
    int MatchesBasculesEffectifIncomplet,
    int MatchesBasculesPaiementManquant,
    int PenalitesAppliquees,
    int SoldesCrees,
    int MatchesCompletes);
