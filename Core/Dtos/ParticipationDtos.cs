namespace Core.Dtos;

public record ParticipationDto(
    int Id,
    int MatchId,
    int? MembreId,
    int NumeroPlace,
    string Role,
    string Statut,
    decimal MontantDu,
    DateTime DateInscription,
    DateTime? DateValidation);

public record AjouterJoueurDto(string Matricule);
