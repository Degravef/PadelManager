namespace Core.Dtos;


public record TraitementQuotidienResultDto(
    DateOnly DateTraitee,
    int MatchesBasculesEffectifIncomplet,
    int MatchesBasculesPaiementManquant,
    int PenalitesAppliquees,
    int SoldesCrees,
    int MatchesCompletes);
