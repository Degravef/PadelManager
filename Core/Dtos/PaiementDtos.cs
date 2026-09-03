namespace Core.Dtos;

public record PaiementDto(
    int Id,
    int MembreId,
    int? ParticipationId,
    int? SoldeDuId,
    decimal Montant,
    DateTime DatePaiement,
    string Statut);

public record PayerDto(string? MoyenPaiement = null);

public record SoldeDuDto(int Id, int MembreId, int MatchId, decimal Montant, string Statut, DateTime DateCreation);
